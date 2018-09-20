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
	public class World
	{
		public Terrain[,,] tiles {get; set;}
		public FastNoise Noise;
		
		public World()
		{
//			noise = Simplex.Noise.Calc2D(Constants.WIDTH, Constants.HEIGHT, 1/32);
			
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			int DEPTH = Constants.DEPTH;
			int GROUNDLEVEL = Constants.GROUNDLEVEL;
			float hscale = 2f;
			float vscale = 5f;
			Noise = new FastNoise();	
			tiles = new Terrain[WIDTH, HEIGHT, DEPTH];
			for (int i=0; i<WIDTH; i++) {
				for (int j=0; j<HEIGHT; j++) {
					for (int k=0; k<DEPTH; k++) {
						int elev = GROUNDLEVEL + (int) (vscale*Noise.GetSimplexFractal(hscale*i,hscale*j));
						if (i==0 || i==WIDTH-1 || j==0 || j==HEIGHT-1 || k<elev) {
							tiles[i,j,k] = Terrains.WallTile;
						} else if (k==elev) {
							tiles[i,j,k] = Terrains.FloorTile;
						} else {
							tiles[i,j,k] = Terrains.EmptyTile;
						}
					}
				}
			}
		}
		
		public int GroundLevel(int x, int y)
		{
			int elev = Constants.DEPTH-1;
			for (int i=Constants.DEPTH-1; i>0; i--) {
				if (tiles[x,y,i].Solid)
				{
					return i+1;
				}
			}
			return 1;
		}
	}
}


//for (var x=1; x<LEVELW-1; x++) {
//      for (var y=1; y<LEVELH-1; y++) {
//        grid[x][y] = base;
//        for (let o=0; o<OCTAVES.length; o++) {
//          grid[x][y]+= noise.get(x/OCTAVES[o],y/OCTAVES[o])*scales[o];
//        }
//      }
//    }
