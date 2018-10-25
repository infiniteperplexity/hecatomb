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
		public SparseArray3D<Creature> Creatures;
		public SparseArray3D<Feature> Features;
		public SparseArray3D<Item> Items;
		public SparseArray3D<TaskEntity> Tasks;
		public Dictionary<string, StateTracker> StateTrackers;
		public HashSet<Coord> Explored;
		public GameEventHandler Events;
		public EntityHandler Entities;
		public Player Player;
		public GameRandom Random;
		
		public FastNoise ElevationNoise;
		public FastNoise VegetationNoise;
		
		public TurnHandler Turns;
		
		public GameWorld() {}
		
		public void Initialize(int width, int height, int depth, object generator=null, int seed=0)
		{
			Random = new GameRandom(seed);
			Entities = new EntityHandler();
			Width = width;
			Height = height;
			Depth = depth;
			int GroundLevel = 50;
			float hscale = 2f;
			float vscale = 5f;
			Events = new GameEventHandler();
			ElevationNoise = new FastNoise(seed: Random.Next(1024));
			VegetationNoise = new FastNoise(seed: Random.Next(1024));
			Tiles = new Terrain[Width, Height, Depth];
			StateTrackers = new Dictionary<string, StateTracker>();
			Creatures = new SparseArray3D<Creature>(Width, Height, Depth);
			Features = new SparseArray3D<Feature>(Width, Height, Depth);
			Items = new SparseArray3D<Item>(Width, Height, Depth);
			Tasks = new SparseArray3D<TaskEntity>(Width, Height, Depth);
			Explored = new HashSet<Coord>();
			for (int i=0; i<Width; i++) {
				for (int j=0; j<Height; j++) {
					for (int k=0; k<Depth; k++) {
						int elev = GroundLevel + (int) (vscale*ElevationNoise.GetSimplexFractal(hscale*i,hscale*j));
						if (i==0 || i==Width-1 || j==0 || j==Height-1 || k<elev) {
							Tiles[i,j,k] = Terrain.WallTile;
						} else if (k==elev) {
							Tiles[i,j,k] = Terrain.FloorTile;
						} else {
							Tiles[i,j,k] = Terrain.EmptyTile;
						}
					}
				}
			}
			for (int i=1; i<Width-1; i++) {
				for (int j=1; j<Height-1; j++) {
					int k = GetGroundLevel(i, j);
					List<Coord> neighbors = Hecatomb.Tiles.GetNeighbors8(i, j, k);
					bool slope = false;
					foreach (Coord c in neighbors)
					{
						if (GetTile(c.X, c.Y, c.Z) == Terrain.WallTile)
						{
							slope = true;
							break;
						}
					}

					if (slope)
					{
						Tiles[i, j, k] = Terrain.UpSlopeTile;
						if (GetTile(i, j, k+1)==Terrain.EmptyTile)
						{
							Tiles[i, j, k+1] = Terrain.DownSlopeTile;
						}
					} else {
						float plants = vscale*VegetationNoise.GetSimplexFractal(hscale*i,hscale*j);
						if (plants>1.0f)
						{
							if (Random.Next(2)==1)
							{
								Feature tree = Entities.Spawn<Feature>("Tree");
								tree.Place(i, j, k);
							}
						}
						else
						{
							if (Random.Next(50)==0)
							{
								Feature grave = Entities.Spawn<Feature>("Grave");
								grave.Place(i, j, k);
							}
						}
					}
				}
			}
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
			List<string> text = new List<string>() {
				"Coord: " + x + "," + y + ", " + z
			};
			TypedEntity t;
			if (Explored.Contains(c))
			{
				text.Add("Terrain: " + Tiles[x, y, z].Name);
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
			if (Explored.Contains(above))
			{
				text.Add("Above: " + Tiles[x, y, za].Name);
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
			if (Explored.Contains(below))
			{
				text.Add("Below: " + Tiles[x, y, zb].Name);
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
			var colors = Game.Controls.BottomColors;
			colors.Clear();
			for (int i=0; i<text.Count; i++)
			{
				colors[i,0] = (i<change) ? main : other;
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