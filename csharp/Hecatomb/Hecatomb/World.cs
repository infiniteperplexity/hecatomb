/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 9:44 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Simplex;
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of World.
	/// </summary>
	public class World
	{
		public int[,] tiles {get; set;}
		public World()
		{
			Simplex.Noise.Seed = 209323094; // Optional
			int length = 10, width = 15;
			float scale = 0.10f;
			float[,] noiseValues = Simplex.Noise.Calc2D(length, width, scale);
			
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			tiles = new int[WIDTH, HEIGHT];
			for (int i=0; i<WIDTH; i++) {
				for (int j=0; j<HEIGHT; j++) {
					if (i==0 || i==WIDTH-1 || j==0 || j==HEIGHT-1) {
						tiles[i,j] = 1;
					} else {
						tiles[i,j] = 0;
					}
				}
			}
		}
	}
}
