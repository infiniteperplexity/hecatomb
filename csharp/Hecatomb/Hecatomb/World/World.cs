/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 9:44 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;


namespace Hecatomb
{
    using static HecatombAliases;
    /// <summary>
    /// GameWorld should contain *all* the truly persistent state for the game - state that's not derived, or part of the GUI.





    public partial class World
	{
        // dimensions
		public int Width;
		public int Height;
		public int Depth;

        // world state
        public Dictionary<int, Entity> Entities
        {
            get
            {
                return Entity.Entities;
            }
        }
        public Terrain[,,] Terrains;
        public Cover[,,] Covers;
        public int[,,] Lighting;
        public int[,,] Outdoors;
		public SparseArray3D<Creature> Creatures;
		public SparseArray3D<Feature> Features;
		public SparseArray3D<Item> Items;
		public SparseArray3D<Task> Tasks;
        public List<ParticleEmitter> Emitters;
        public SparseJaggedArray3D<Particle> Particles;
        public Dictionary<string, StateHandler> StateHandlers;
		public HashSet<Coord> Explored;
		public EventHandler Events;
		public Creature Player;
		public StatefulRandom Random;
        public TurnHandler Turns;

        // should these attach to builder?
        public FastNoise ElevationNoise;
		public FastNoise VegetationNoise;
		
		
		
		public World(int width, int height, int depth, int seed=0)
        { 
            // create one of each 
            var statehandlers = typeof(StateHandler).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StateHandler))).ToList();
            foreach (var handler in statehandlers)
            {
                Activator.CreateInstance(handler);
            }

			Random = new StatefulRandom(seed);
            Entity.Reset();
			Width = width;
			Height = height;
			Depth = depth;
			Events = new EventHandler();
            Lighting = new int[Width, Height, Depth];
            Outdoors = new int[Width, Height, Depth];
            Terrains = new Terrain[Width, Height, Depth];
            Covers = new Cover[Width, Height, Depth];
			StateHandlers = new Dictionary<string, StateHandler>();
			Creatures = new SparseArray3D<Creature>(Width, Height, Depth);
			Features = new SparseArray3D<Feature>(Width, Height, Depth);
			Items = new SparseArray3D<Item>(Width, Height, Depth);
			Tasks = new SparseArray3D<Task>(Width, Height, Depth);
            Particles = new SparseJaggedArray3D<Particle>(Width, Height, Depth);
            Emitters = new List<ParticleEmitter>();
			Explored = new HashSet<Coord>();
			Turns = new TurnHandler();
            
		}
		
		public void Reset()
		{
            // Random = new GameRandom(Random.Seed);
            Entity.Reset();
			Events = new EventHandler();
			Terrains = new Terrain[Width, Height, Depth];
            Covers = new Cover[Width, Height, Depth];
			StateHandlers = new Dictionary<string, StateHandler>();
			Creatures = new SparseArray3D<Creature>(Width, Height, Depth);
			Features = new SparseArray3D<Feature>(Width, Height, Depth);
			Items = new SparseArray3D<Item>(Width, Height, Depth);
			Tasks = new SparseArray3D<Task>(Width, Height, Depth);
            Particles = new SparseJaggedArray3D<Particle>(Width, Height, Depth);
            Emitters = new List<ParticleEmitter>();
            Explored = new HashSet<Coord>();
			Turns = new TurnHandler(); 
		}
		
		public Terrain GetTile(int x, int y, int z)
		{
			if (x<0 || x>=Width || y<0 || y>=Height || z<0 || z>=Depth) {
				return Terrain.OutOfBoundsTile;
			} else {
				return Terrains[x,y,z];
			}
		}
		
		public int GetGroundLevel(int x, int y)
		{
			if (x<=0 || x>=Width-1 || y<=0 || y>=Height-1) {
				throw new IndexOutOfRangeException(String.Format("Cannot get GroundLevel for boundary column {0} {1}.",x,y));
			}
			int elev = Depth-1;
			for (int i=Depth-1; i>0; i--) {
				if (Terrains[x,y,i].Solid)
				{
					return i+1;
				}
			}
			return 1;
		}

		public void ShowTileDetails(Coord c)
		{
			int x = c.X;
			int y = c.Y;
			int z = c.Z;
			int za = z+1;
			int zb = z-1;
			Coord above = new Coord(x, y, za);
			Coord below = new Coord(x, y, zb);
			string main = "light cyan";
			string other = "gainsboro";
			int change = 0;
			List<ColoredText> text = new List<ColoredText>() {
				"Coord: " + x + "," + y + ", " + z
			};
			TileEntity t;
			if (Explored.Contains(c) || Game.Options.Explored)
			{
				text.Add("Terrain: " + Terrains[x, y, z].Name);
                text.Add("Cover: " + Covers[x, y, z].Name);
				t = Creatures[x, y, z];
				if (t!=null)
				{
					text.Add("Creature: " + t.Describe());
				}
				t = Features[x, y, z];
				if (t!=null)
				{
					text.Add("Feature: " + t.Describe());
				}
				t = Tasks[x, y, z];
				if (t!=null)
				{
					text.Add("Task: " + t.Describe());
				}
				text.Add(" ");
			}
			change = text.Count;
			if (Explored.Contains(above) || Game.Options.Explored)
			{
				text.Add("Above: " + Terrains[x, y, za].Name);
                text.Add("Cover: " + Covers[x, y, za].Name);
                t = Creatures[x, y, za];
				if (t!=null)
				{
					text.Add("Creature: " + t.Describe());
				}
				t = Features[x, y, za];
				if (t!=null)
				{
					text.Add("Feature: " + t.Describe());
				}
				t = Tasks[x, y, za];
				if (t!=null)
				{
					text.Add("Task: " + t.Describe());
				}
				text.Add(" ");
			}
			if (Explored.Contains(below) || Game.Options.Explored)
			{
				text.Add("Below: " + Terrains[x, y, zb].Name);
                text.Add("Cover: " + Covers[x, y, zb].Name);
                t = Creatures[x, y, zb];
				if (t!=null)
				{
					text.Add("Creature: " + t.Describe());
				}
				t = Features[x, y, zb];
				if (t!=null)
				{
					text.Add("Feature: " + t.Describe());
				}
				t = Tasks[x, y, zb];
				if (t!=null)
				{
					text.Add("Task: " +t.Describe());
				}
				text.Add(" ");
			}
			Game.Controls.MenuBottom = text;
			for (int i=0; i<text.Count; i++)
			{
                Game.Controls.MenuBottom[i].Colors[0] = (i<change) ? main : other;
			}
			Game.MenuPanel.Dirty = true;
		}
		
		public T GetState<T>() where T : StateHandler, new()
		{
			string s = typeof(T).Name;
			if (!StateHandlers.ContainsKey(s))
			{
				var tracker = Entity.Spawn<T>();
				tracker.Activate();
			}
			return (T) StateHandlers[s];
		}

        public void ValidateOutdoors()
        {
            GetState<PathHandler>().ResetPaths();
            Outdoors = new int[Width, Height, Depth];
            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {

                    for (int z = Depth - 1; z > 0; z--)
                    {
                        Outdoors[x, y, z] = 2;
                        foreach (Coord dir in Movement.Directions8)
                        {
                            var (dx, dy, _) = dir;
                            if (Outdoors[x+dx, y+dy, z]==0)
                            {
                                Outdoors[x + dx, y + dy, z] = 1;
                            }
                        }
                        if (Terrains[x, y, z].Solid)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public void ValidateLighting()
        {
          
        }
	}
}