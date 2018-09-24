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
	public struct Coord {
		public int x;
		public int y;
		public int z;
		
		public Coord(int _x, int _y, int _z)
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
		
		public readonly static Coord North = new Coord(+0,-1,+0);
		public readonly static Coord South = new Coord(+0,-1,+0);
		public readonly static Coord East = new Coord(+1,+0,+0);
		public readonly static Coord West = new Coord(-1,+0,+0);
		public readonly static Coord NorthEast = new Coord(+1,-1,+0);
		public readonly static Coord SouthEast = new Coord(+1,-1,+0);
		public readonly static Coord NorthWest = new Coord(-1,-1,+0);
		public readonly static Coord SouthWest = new Coord(-1,+1,+0);
		public readonly static Coord UpNorth = new Coord(+0,-1,+1);
		public readonly static Coord UpSouth = new Coord(+0,-1,+1);
		public readonly static Coord UpEast = new Coord(+1,+0,+1);
		public readonly static Coord UpWest = new Coord(-1,+0,+1);
		public readonly static Coord UpNorthEast = new Coord(+1,-1,+1);
		public readonly static Coord UpSouthEast = new Coord(+1,-1,+1);
		public readonly static Coord UpNorthWest = new Coord(-1,-1,+1);
		public readonly static Coord UpSouthWest = new Coord(-1,+1,+1);
		public readonly static Coord DownNorth = new Coord(+0,-1,-1);
		public readonly static Coord DownSouth = new Coord(+0,-1,-1);
		public readonly static Coord DownEast = new Coord(+1,+0,-1);
		public readonly static Coord DownWest = new Coord(-1,+0,-1);
		public readonly static Coord DownNorthEast = new Coord(+1,-1,-1);
		public readonly static Coord DownSouthEast = new Coord(+1,-1,-1);
		public readonly static Coord DownNorthWest = new Coord(-1,-1,-1);
		public readonly static Coord DownSouthWest = new Coord(-1,+1,-1);
		public readonly static Coord Up = new Coord(+0,+0,+1);
		public readonly static Coord Down = new Coord(+0,+0,-1);
		
		public static Coord[] Directions4 = new Coord[] {
			North,
			South,
			East,
			West			
		};
		
		public static Coord[] Directions8 = new Coord[] {
			North,
			South,
			East,
			West,
			NorthEast,
			SouthEast,
			NorthWest,
			SouthWest			
		};
		
		public static Coord[] Directions26 = new Coord[] {
			North,
			South,
			East,
			West,
			NorthEast,
			SouthEast,
			NorthWest,
			SouthWest,
			UpNorth,
			UpSouth,
			UpEast,
			UpWest,
			UpNorthEast,
			UpSouthEast,
			UpNorthWest,
			UpSouthWest,
			DownNorth,
			DownSouth,
			DownEast,
			DownWest,
			DownNorthEast,
			DownSouthEast,
			DownNorthWest,
			DownSouthWest,
			Up,
			Down
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
