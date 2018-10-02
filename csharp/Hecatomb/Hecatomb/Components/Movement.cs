/*
 * Created by SharpDevelop.
 * User: Glenn Wright
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
		public bool Phases;
		
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
		
		public static Coord[] Directions10 = new Coord[] {
			North,
			South,
			East,
			West,
			NorthEast,
			SouthEast,
			NorthWest,
			SouthWest,
			Up,
			Down			
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
		
		public Movement(): base()
		{
			Walks = true;
			Climbs = true;
			Flies = false;
			Swims = true;
			Phases = false;
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
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH) {
				return false;
			}
			Terrain tile = Game.World.Tiles[x1,y1,z1];
			// non-phasers can't go through a solid wall
			if (!Phases && tile.Solid) {
				return false;
			}
			// non-flyers can't cross a pit
			if (tile.Fallable && Flies==false) {
				return false;
			}
			int dx = x1-Entity.x;
			int dy = y1-Entity.y;
			int dz = z1-Entity.z;
			// rare: check whether the square itself is allowed
			if (dx==0 && dy==0 && dz==0)
			{
				return true;
			}
			// non-flyers can't climb diagonally
			if (!Flies && dz!=0 && (dx!=0 || dy!=0))
			{
				return false;
			}	
			// non-flyers need a slope in order to go up
			Terrain t0 = Game.World.Tiles[Entity.x, Entity.y, Entity.z];
			if (dz==+1 && !Flies && t0.ZWalk!=+1)
			{
				return false;
      		}
			// non-phasers can't go through a ceiling
			if (!Phases)
			{
				if (dz==+1 && !tile.Fallable && tile.ZWalk!=-1)
				{
					return false;
				}
				else if (dz==-1 && t0.ZWalk!=-1 && !t0.Fallable)
				{
					return false;
				}
			}
			if (Walks)
			{
				return true;
			}
			if (Flies)
			{
				return true;
			}
			// needs changed once we add water
			if (Swims)
			{
				return true;
			}
			return false;
		}
		
		public void StepTo(int x1, int y1, int z1)
		{
			Entity.Place(x1, y1, z1);
		}
	}
}
