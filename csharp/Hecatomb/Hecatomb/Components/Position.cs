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
		public int x {get; private set;}
		public int y {get; private set;}
		public int z {get; private set;}
		public bool Placed {get; private set;}
		
		public Position()
		{
			Layer = WorldLayer.Creatures;
			Placed = false;
		}
		
		public void Place(int x1, int y1, int z1)
		{
			if (Layer==WorldLayer.Creatures){
				if (Placed) {
					Remove();
				}
				if (Game.World.Creatures[x1,y1,z1]==null)
				{
					Placed = true;
					Game.World.Creatures[x1,y1,z1] = Entity;
					x = x1;
					y = y1;
					z = z1;
				} else {
					throw new InvalidOperationException();
				}
			}
		}
		public void Remove()
		{
			Placed = false;
			if (Layer==WorldLayer.Creatures) {
				Game.World.Creatures[x,y,z] = null;
			}
		}
	}
	
	
}
