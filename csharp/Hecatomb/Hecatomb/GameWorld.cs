/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 9:44 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of World.
	/// </summary>
	public class GameWorld
	{
		public Terrain[,,] Tiles {get; set;}
		public SparseArray3D<Entity> Creatures;
		public SparseArray3D<Entity> Tasks;
		
		public FastNoise Noise;
		
		public GameWorld()
		{
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			int DEPTH = Constants.DEPTH;
			int GROUNDLEVEL = Constants.GROUNDLEVEL;
			float hscale = 2f;
			float vscale = 5f;
			Noise = new FastNoise(seed: Game.Random.Next(1024));
			Tiles = new Terrain[WIDTH, HEIGHT, DEPTH];
			Creatures = new SparseArray3D<Entity>(WIDTH, HEIGHT, DEPTH);
			Tasks = new SparseArray3D<Entity>(WIDTH, HEIGHT, DEPTH);
			for (int i=0; i<WIDTH; i++) {
				for (int j=0; j<HEIGHT; j++) {
					for (int k=0; k<DEPTH; k++) {
						int elev = GROUNDLEVEL + (int) (vscale*Noise.GetSimplexFractal(hscale*i,hscale*j));
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
		}
		
//		public Terrain GetTile(int x, int y, int z)
//		{
//			if (x<0 || x>=Constants.WIDTH || y<0 || y>=Constants.HEIGHT || z<0 || z>=Constants.DEPTH) {
//				return null;
//			} else {
//				return Tiles[x,y,z];
//			}
//		}
		public int GroundLevel(int x, int y)
		{
			int elev = Constants.DEPTH-1;
			for (int i=Constants.DEPTH-1; i>0; i--) {
				if (Tiles[x,y,i].Solid)
				{
					return i+1;
				}
			}
			return 1;
		}
	}
}