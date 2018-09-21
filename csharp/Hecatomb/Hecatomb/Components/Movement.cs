/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 11:33 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Movement.
	/// </summary>
	/// 
	public struct Direction {
		public int x;
		public int y;
		public int z;
		
		public Direction(int _x, int _y, int _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}
	}
	
	public class Movement : Component
	{
		public bool Walks;
		public bool Climbs;
		public bool Flies;
		public bool Swims;
		
		public readonly static Direction North = new Direction(+0,-1,+0);
		public readonly static Direction South = new Direction(+0,-1,+0);
		public readonly static Direction East = new Direction(+1,+0,+0);
		public readonly static Direction West = new Direction(-1,+0,+0);
		public readonly static Direction Up = new Direction(+0,+0,+1);
		public readonly static Direction Down = new Direction(+0,+0,+1);
		
		public static Direction[] Directions4 = new Direction[] {
			North,
			South,
			East,
			West			
		};
		
		public Movement()
		{
			Required = new string[] {"Position"};
			Walks = true;
			Climbs = true;
			Flies = false;
			Swims = true;
		}
		
		public bool CanPass(int x1, int y1, int z1)
		{
			if (!CanMove(x1, y1, z1))
			{
				return false;
			}
			if (Game.World.Creatures[x1,y1,z1]!=null)
			{
				return false;
			}
			return true;
		}
		
		public bool CanMove(int x1, int y1, int z1)
		{
			// there are situations where diagonal movement should be blocked...
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH) {
				return false;
			}
			Terrain tile = Game.World.Tiles[x1,y1,z1];
			
			if (tile.Solid) {
				return false;
			}
			if (tile.Fallable && Flies==false) {
				return false;
			}
			return true;
		}
		
		public void StepTo(int x1, int y1, int z1)
		{
			Entity.GetComponent<Position>().Place(x1, y1, z1);
		}
	}
}
