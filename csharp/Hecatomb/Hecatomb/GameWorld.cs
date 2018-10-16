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
		public Terrain[,,] Tiles {get; set;}
		public SparseArray3D<Creature> Creatures;
		public SparseArray3D<Feature> Features;
		public SparseArray3D<Item> Items;
		public SparseArray3D<TaskEntity> Tasks;
		public HashSet<Coord> Explored;
		public GameEventHandler Events;
		public EntityHandler Entities;
		public Player Player;
		public GameRandom Random;
		
		public FastNoise ElevationNoise;
		public FastNoise VegetationNoise;
		
		public TurnHandler Turns;
		
		public GameWorld() {}
		
		public void Initialize(int seed)
		{
			Random = new GameRandom(seed);
			Entities = new EntityHandler();
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			int DEPTH = Constants.DEPTH;
			int GROUNDLEVEL = Constants.GROUNDLEVEL;
			float hscale = 2f;
			float vscale = 5f;
			Events = new GameEventHandler();
			ElevationNoise = new FastNoise(seed: Random.Next(1024));
			VegetationNoise = new FastNoise(seed: Random.Next(1024));
			Tiles = new Terrain[WIDTH, HEIGHT, DEPTH];
			Creatures = new SparseArray3D<Creature>(WIDTH, HEIGHT, DEPTH);
			Features = new SparseArray3D<Feature>(WIDTH, HEIGHT, DEPTH);
			Items = new SparseArray3D<Item>(WIDTH, HEIGHT, DEPTH);
			Tasks = new SparseArray3D<TaskEntity>(WIDTH, HEIGHT, DEPTH);
			Explored = new HashSet<Coord>();
			for (int i=0; i<WIDTH; i++) {
				for (int j=0; j<HEIGHT; j++) {
					for (int k=0; k<DEPTH; k++) {
						int elev = GROUNDLEVEL + (int) (vscale*ElevationNoise.GetSimplexFractal(hscale*i,hscale*j));
						if (i==0 || i==WIDTH-1 || j==0 || j==HEIGHT-1 || k<elev) {
							Tiles[i,j,k] = Terrain.WallTile;
						} else if (k==elev) {
							Tiles[i,j,k] = Terrain.FloorTile;
						} else {
							Tiles[i,j,k] = Terrain.EmptyTile;
						}
					}
				}
			}
			for (int i=1; i<WIDTH-1; i++) {
				for (int j=1; j<HEIGHT-1; j++) {
					int k = GroundLevel(i, j);
					List<Coord> neighbors = Hecatomb.Tiles.GetNeighbors8(i, j, k);
					bool slope = false;
					foreach (Coord c in neighbors)
					{
						if (GetTile(c.x, c.y, c.z) == Terrain.WallTile)
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
			if (x<0 || x>=Constants.WIDTH || y<0 || y>=Constants.HEIGHT || z<0 || z>=Constants.DEPTH) {
				return Terrain.OutOfBoundsTile;
			} else {
				return Tiles[x,y,z];
			}
		}
		
		public int GroundLevel(int x, int y)
		{
			if (x<=0 || x>=Constants.WIDTH-1 || y<=0 || y>=Constants.HEIGHT-1) {
				throw new IndexOutOfRangeException(String.Format("Cannot get GroundLevel for boundary column {0} {1}.",x,y));
			}
			int elev = Constants.DEPTH-1;
			for (int i=Constants.DEPTH-1; i>0; i--) {
				if (Tiles[x,y,i].Solid)
				{
					return i+1;
				}
			}
			return 1;
		}

		public void ShowTileDetails(Coord c)
		{
			int x = c.x;
			int y = c.y;
			int z = c.z;
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
					text.Add("Creature: " + t.Name);
				}
				t = Features[x, y, z];
				if (t!=null)
				{
					text.Add("Feature: " + t.Name);
				}
				t = Tasks[x, y, z];
				if (t!=null)
				{
					text.Add("Task: " + t.Name);
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
					text.Add("Creature: " + t.Name);
				}
				t = Features[x, y, za];
				if (t!=null)
				{
					text.Add("Feature: " + t.Name);
				}
				t = Tasks[x, y, za];
				if (t!=null)
				{
					text.Add("Task: " + t.Name);
				}
				text.Add(" ");
			}
			if (Explored.Contains(below))
			{
				text.Add("Below: " + Tiles[x, y, zb].Name);
				t = Creatures[x, y, zb];
				if (t!=null)
				{
					text.Add("Creature: " + t.Name);
				}
				t = Features[x, y, zb];
				if (t!=null)
				{
					text.Add("Feature: " + t.Name);
				}
				t = Tasks[x, y, zb];
				if (t!=null)
				{
					text.Add("Task: " + t.Name);
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
	}
}