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
        public static bool WorldSafeToDraw;
        // shouldn't this be in WorldBuilder, not world?
		public Creature PlacePlayer()
        {
            bool valid = true;
            var x = Width / 2;
            var y = Height / 2;
            var z = GetGroundLevel(Width / 2, Height / 2);
            if(Covers[x, y, z].Liquid)
            {
                valid = false;
            }
            var nearbyGraves = 0;
            var graves = 0;
            var trees = 0;
            foreach (Feature f in Features)
            {
                if (f.TypeName=="Grave" && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z)<=15)
                {
                    graves += 1;
                    if (Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <= 5 && z==f.Z)
                    {
                        nearbyGraves += 1;
                    }
                }
                else if ((f.TypeName == "ClubTree" || f.TypeName == "SpadeTree") && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <= 15)
                {
                    trees += 1;
                }
            }
            if (nearbyGraves<1 || graves<3 || trees < 5)
            {
                valid = false;
            }
            var failures = 0;
            while (!valid)
            {
                failures += 1;
                if (failures>=250)
                {
                    Debug.WriteLine("had trouble placing player, regenerating world.");
                    return null;
                }
                x += Random.Next(-5, 5);
                y += Random.Next(-5, 5);
                z = GetGroundLevel(x, y);
                valid = true;
                if (Covers[x, y, z].Liquid)
                {
                    valid = false;
                }
                nearbyGraves = 0;
                graves = 0;
                foreach (Feature f in Features)
                {
                    if (f.TypeName == "Grave" && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <= 15)
                    {
                        graves += 1;
                        if (Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <= 5 && z==f.Z)
                        {
                            nearbyGraves += 1;
                        }
                    }
                    else if ((f.TypeName == "ClubTree" || f.TypeName == "SpadeTree") && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <= 15)
                    {
                        trees += 1;
                    }
                }
                if (nearbyGraves < 1 || graves < 3 || trees < 5)
                {
                    valid = false;
                }
            }
            int surfaceFlint = 0;
            int surfaceCoal = 0;
            for (int i = -15; i <= 15; i++)
            {
                for (int j = -15; j <= 15; j++)
                {
                    int k = GetGroundLevel(x + i, y + j);
                    if (Covers[x + i, y + j, k - 1] == Cover.FlintCluster)
                    {
                        surfaceFlint += 1;
                    }
                    else if (Covers[x + i, y + j, k - 1] == Cover.CoalSeam)
                    {
                        surfaceCoal += 1;
                    }
                }
            }
            Debug.WriteLine("nearby surface flint: " + surfaceFlint);
            Debug.WriteLine("nearby surface coal: " + surfaceCoal);
            while (surfaceFlint < 15)
            {
                int rx = x - 15 + Game.World.Random.Next(31);
                int ry = y - 15 + Game.World.Random.Next(31);
                if (Covers[rx, ry, GetGroundLevel(rx, ry) - 1] == Cover.Soil)
                {
                    Covers[rx, ry, GetGroundLevel(rx, ry) - 1] = Cover.FlintCluster;
                    surfaceFlint += 1;
                }
            }
            while (surfaceCoal < 15)
            {
                int rx = x - 15 + Game.World.Random.Next(31);
                int ry = y - 15 + Game.World.Random.Next(31);
                if (Covers[rx, ry, GetGroundLevel(rx, ry) - 1] == Cover.Soil)
                {
                    Covers[rx, ry, GetGroundLevel(rx, ry) - 1] = Cover.CoalSeam;
                    surfaceCoal += 1;
                }
            }
            if (Creatures[x,y,z]!=null)
            {
                Creatures[x, y, z].Despawn();
            }
            Creature p = Hecatomb.Entity.Spawn<Creature>("Necromancer");
            Player = p;
            p.GetComponent<Actor>().Team = Teams.Friendly;
            p.Place(x, y, z);
            //***** This logic absolutely should not go here
            int goodsBounds = 12;
            for (int i = x-goodsBounds; i<=x+goodsBounds; i++)
            {
                for (int j = y-goodsBounds; j<=y+goodsBounds; j++)
                {
                    Feature f = Features[i, j, GetGroundLevel(i, j)];
                    if (f!=null && f.TypeName=="Grave")
                    {
                        f.GetComponent<Harvestable>().Yields.Remove("TradeGoods");
                    }
                }
            }
            //*****
            Game.Camera.Center(x, y, z);
            foreach (var res in Game.Options.FreeStuff)
            {
                Item.PlaceNewResource(res.Item1, res.Item2, x, y, z);
            }
            return p;
        }
		


		public World(int width, int height, int depth, int seed=0)
        {
            WorldSafeToDraw = false;
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
            InitializeHandlers();
        }
		
		public void Reset()
		{
            WorldSafeToDraw = false;
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
            InitializeHandlers();
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
                if (Terrains[x,y,i]==Terrain.FloorTile || Terrains[x,y,i]==Terrain.UpSlopeTile)
                {
                    return i;
                }
				//if (Terrains[x,y,i].Solid)
				//{
				//	return i+1;
				//}
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
                text.Add("Lighting: " + GetLighting(x, y, z));
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
                t = Items[x, y, z];
                if (t!=null)
                {
                    text.Add("Item: " + t.Describe());
                }
				t = Tasks[x, y, z];
				if (t!=null)
				{
                    //text.Add("Task: " + t.Describe(article: false));
                    text.Add("Task: " + (t as Task).GetHoverName());
                }
				text.Add(" ");
			}
			change = text.Count;
			if (Explored.Contains(above) || Game.Options.Explored)
			{
				text.Add("Above: " + Terrains[x, y, za].Name);
                text.Add("Cover: " + Covers[x, y, za].Name);
                text.Add("Lighting: " + GetLighting(x, y, za));
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
                t = Items[x, y, za];
                if (t != null)
                {
                    text.Add("Item: " + t.Describe());
                }
                t = Tasks[x, y, za];
				if (t!=null)
				{
					text.Add("Task: " + (t as Task).GetHoverName());
				}
				text.Add(" ");
			}
			if (Explored.Contains(below) || Game.Options.Explored)
			{
				text.Add("Below: " + Terrains[x, y, zb].Name);
                text.Add("Cover: " + Covers[x, y, zb].Name);
                text.Add("Lighting: " + GetLighting(x, y, zb));
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
                t = Items[x, y, zb];
                if (t != null)
                {
                    text.Add("Item: " + t.Describe());
                }
                t = Tasks[x, y, zb];
				if (t!=null)
				{
					text.Add("Task: " + (t as Task).GetHoverName());
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

        // this violates every principle of OO programming but it prevents me from having to manually initialize every statehandler
        public void InitializeHandlers()
        {
            List<Type> handlers = typeof(StateHandler).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StateHandler))).ToList();
            foreach (var type in handlers)
            {
                InitializeHandler(type);
            }
        }
        public StateHandler InitializeHandler(Type t)
        {
            string s = t.Name;
            if (!StateHandlers.ContainsKey(s))
            {
                StateHandler sh = (StateHandler)Activator.CreateInstance(t);
                sh.EID = Entity.MaxEID + 1;
                Entity.MaxEID = sh.EID;
                Entities[sh.EID] = sh;
                sh.Spawned = true;
                StateHandlers[s] = sh;
                foreach (Type type in sh.Listeners.Keys)
                {
                    Events.Subscribe(type, sh, sh.Listeners[type]);
                }   
            }
            return StateHandlers[s];
        }


        public void ValidateOutdoors()
        {
            GetState<PathHandler>().ResetPaths();
            Outdoors = new int[Width, Height, Depth];
            // by default it's full of 0s (indoors)
            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {

                    for (int z = Depth - 1; z > 0; z--)
                    {

                        // everything above ground is outdoors (2)
                        Outdoors[x, y, z] = 2;
                        foreach (Coord dir in Movement.Directions8)
                        {
                            var (dx, dy, _) = dir;
                            // this gets really weird because it tags walls as shaded.  This may change in the future.
                            if (Outdoors[x + dx, y + dy, z] == 0)
                            {
                                // if this square is outdoors, tag adjacent indoor squares as shaded (1)                      
                                Outdoors[x + dx, y + dy, z] = 1;
                            }
                        }
                        if (Terrains[x, y, z].ZView!=-1)
                        {
                            //Debug.WriteLine("breaking off at " + z);
                            //Debug.WriteLine(Outdoors[x, y, z]);
                            break;
                        } 
                    }
                }
            }
        }
        public void ValidateLighting()
        {
          
        }
        public int GetLighting(int x, int y, int z)
        {
            int lighting = Game.World.Turns.LightLevel;
            int outdoors = Game.World.Outdoors[x, y, z];
            if (outdoors == 0)
            {
                lighting = 0;
            }
            else if (outdoors == 1)
            {
                lighting = lighting / 2;
            }
            return lighting;
        }
	}
}