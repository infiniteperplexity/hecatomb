/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 11:34 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Position.
	/// </summary>
	
	public enum WorldLayer {
		Terrain, // usually background and foreground
		Cover, // usually background and foreground
		Features, // always foreground, sometimes background
		Items, // foreground
		Creatures, // foreground
		Zones, // background
		Particles, // usually foreground, sometimes background
		Interface // usually background, sometimes foreground
	}
	
	public class Position : Component
	{
		public WorldLayer Layer;
		public int x;
		public int y;
		public int z;
		
		public void Place(int x, int y, int z)
		{
			if (Layer==WorldLayer.Creatures){
				
			}
		}
	}
	
	
}
