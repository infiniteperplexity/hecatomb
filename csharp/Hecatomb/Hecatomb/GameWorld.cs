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

namespace Hecatomb
{
	/// <summary>
	/// GameWorld should contain *all* the truly persistent state for the game - state that's not derived, or part of the GUI.
	/// </summary>
	public partial class GameWorld
	{
		public int Width;
		public int Height;
		public int Depth;
		
		public Terrain[,,] Tiles {get; set;}
        public Cover[,,] Covers {get; set;}
		public SparseArray3D<Creature> Creatures;
		public SparseArray3D<Feature> Features;
		public SparseArray3D<Item> Items;
		public SparseArray3D<TaskEntity> Tasks;
        public List<ParticleEmitter> Emitters;
        public SparseJaggedArray3D<Particle> Particles;
        public Dictionary<string, StateTracker> StateTrackers;
		public HashSet<Coord> Explored;
		public GameEventHandler Events;
		public EntityHandler Entities;
		public Player Player;
		public GameRandom Random;
		
		public FastNoise ElevationNoise;
		public FastNoise VegetationNoise;
		
		public TurnHandler Turns;
		
		public GameWorld(int width, int height, int depth, int seed=0) {
			Random = new GameRandom(seed);
			Entities = new EntityHandler();
			Width = width;
			Height = height;
			Depth = depth;
			Events = new GameEventHandler();
			Tiles = new Terrain[Width, Height, Depth];
            Covers = new Cover[Width, Height, Depth];
			StateTrackers = new Dictionary<string, StateTracker>();
			Creatures = new SparseArray3D<Creature>(Width, Height, Depth);
			Features = new SparseArray3D<Feature>(Width, Height, Depth);
			Items = new SparseArray3D<Item>(Width, Height, Depth);
			Tasks = new SparseArray3D<TaskEntity>(Width, Height, Depth);
            Particles = new SparseJaggedArray3D<Particle>(Width, Height, Depth);
            Emitters = new List<ParticleEmitter>();
			Explored = new HashSet<Coord>();
			Turns = new TurnHandler();
            
		}
		
		public void Reset()
		{
			// Random = new GameRandom(Random.Seed);
			Entities = new EntityHandler();
			Events = new GameEventHandler();
			Tiles = new Terrain[Width, Height, Depth];
            Covers = new Cover[Width, Height, Depth];
			StateTrackers = new Dictionary<string, StateTracker>();
			Creatures = new SparseArray3D<Creature>(Width, Height, Depth);
			Features = new SparseArray3D<Feature>(Width, Height, Depth);
			Items = new SparseArray3D<Item>(Width, Height, Depth);
			Tasks = new SparseArray3D<TaskEntity>(Width, Height, Depth);
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
				return Tiles[x,y,z];
			}
		}
		
		public int GetGroundLevel(int x, int y)
		{
			if (x<=0 || x>=Width-1 || y<=0 || y>=Height-1) {
				throw new IndexOutOfRangeException(String.Format("Cannot get GroundLevel for boundary column {0} {1}.",x,y));
			}
			int elev = Depth-1;
			for (int i=Depth-1; i>0; i--) {
				if (Tiles[x,y,i].Solid)
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
			PositionedEntity t;
			if (Explored.Contains(c) || Game.Options.Explored)
			{
				text.Add("Terrain: " + Tiles[x, y, z].Name);
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
				text.Add("Above: " + Tiles[x, y, za].Name);
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
				text.Add("Below: " + Tiles[x, y, zb].Name);
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
		
		public T GetTracker<T>() where T : StateTracker, new()
		{
			string s = typeof(T).Name;
			if (!StateTrackers.ContainsKey(s))
			{
				var tracker = Entities.Spawn<T>();
				tracker.Activate();
			}
			return (T) StateTrackers[s];
		}
	}
}