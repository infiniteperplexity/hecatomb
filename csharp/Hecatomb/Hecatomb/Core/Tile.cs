/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 3:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of AStar.
	public static partial class Tiles
	{
		
		private static bool defaultStandable(int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Game.World.Width || y1<0 || y1>=Game.World.Height || z1<0 || z1>=Game.World.Depth)
			{
      			return false;
    		}
			Terrain t = Game.World.Terrains[x1,y1,z1];
			return (!t.Solid && !t.Fallable);
		}

        private static bool sameSquare(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            return (x0==x1 && y0==y1 && z0==z1);
        }
        private static bool defaultMovable(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Game.World.Width || y1<0 || y1>=Game.World.Height || z1<0 || z1>=Game.World.Depth)
			{
      			return false;
    		}
			Terrain t = Game.World.Terrains[x1,y1,z1];
			return (!t.Solid && !t.Fallable);
		}
		
		public static LinkedList<Coord> FindPath(
			Movement m, TileEntity t, bool useLast=true)
		{
			var misses = Game.World.GetState<PathHandler>().PathMisses;
			if (misses.ContainsKey(m.Entity.EID) && misses[m.Entity.EID].ContainsKey(t.EID))
			{
				return new LinkedList<Coord>();
			}
			int x0 = m.Entity.X;
			int y0 = m.Entity.Y;
			int z0 = m.Entity.Z;
			int x1 = t.X;
			int y1 = t.Y;
			int z1 = t.Z;
			Func<int, int, int, int, int, int, bool> movable = (int x, int y, int z, int xx, int yy, int zz)=>
			{
				return m.CouldMove(x, y, z, xx, yy, zz);
			};
			Func<int, int, int, bool> standable = (int x, int y, int z)=>
			{
				return m.CanStand(x, y, z);
			};
            Func<int, int, int, int, int, int, bool> condition;

            if (useLast)
            {
                condition = sameSquare;
            }
            else
            {
                condition = (int x, int y, int z, int xx, int yy, int zz) =>
                {
                    return m.CanTouch(x, y, z, xx, yy, zz);
                };
            }
            var path = FindPath(
				x0, y0, z0, x1, y1, z1,
				condition: condition,
				movable: movable,
				standable: standable,
                useLast: useLast
			);
			if (path.Count==0)
			{
				Debug.Print("{0} failed to find a path to {1} at {2} {3} {4}", m.Entity.Entity.Describe(),t.Describe(), t.X, t.Y, t.Z);
				if (!misses.ContainsKey(m.Entity.EID))
				{
					misses[m.Entity.EID] = new Dictionary<int, int>();
				}
				misses[m.Entity.EID][t.EID] = 10;
			}
			else
			{
				Debug.Print(m.Entity.Describe() + " found a path to " + t.Describe());
//				pathHits[m.Entity.EID][t.EID] = 10;
			}
			return path;
		}
		
		public static LinkedList<Coord> FindPath(
			Movement m, int x1, int y1, int z1, bool useLast = true)
		{
			int x0 = m.Entity.X;
			int y0 = m.Entity.Y;
			int z0 = m.Entity.Z;
			Func<int, int, int, int, int, int, bool> movable = (int x, int y, int z, int xx, int yy, int zz)=>
			{
				return m.CouldMove(x, y, z, xx, yy, zz);
			};
			Func<int, int, int, bool> standable = (int x, int y, int z)=>
			{
				return m.CanStand(x, y, z);
			};
            Func<int, int, int, int, int, int, bool> condition;
            if (useLast)
            {
                condition = sameSquare;
            }
            else
            {
                condition = (int x, int y, int z, int xx, int yy, int zz) =>
                { 
                    return m.CanTouch(x, y, z, xx, yy, zz);
                };
            }
            return FindPath(
				x0, y0, z0, x1, y1, z1,
                useLast: useLast,
				condition: condition,
				movable: movable,
				standable: standable
			);
		}
		public static LinkedList<Coord> FindPath(
//		public static Coord? FindPath(
			int x0, int y0, int z0, int x1, int y1, int z1,
		    bool useFirst = false,
            bool useLast = true,
			Func<int, int, int, int, int, int, double> heuristic = null,
            Func<int, int, int, int, int, int, bool> condition = null,
            Func<int, int, int, int, int, int, bool> movable = null,
			Func<int, int, int, bool> standable = null
		) {
//			Debug.WriteLine("Finding path from {0} {1} {2} to {3} {4} {5}",x0,y0,z0,x1,y1,z1);
			// default value for the cost estimation heuristic
			heuristic = heuristic ?? QuickDistance;
            // default value for allowable movement
            condition = condition ?? sameSquare;
			movable = movable ?? defaultMovable;
			standable = standable ?? defaultStandable;
			// !should check enclosed right up front
			// !this doesn't have to be shuffled but it would be nice if it were
			Coord[] dirs = Movement.Directions10.OrderBy((Coord c)=>Game.World.Random.NextDouble()).ToArray();
			//Coord[] dirs = Game.Random.Shuffled(Movement.Directions26);
			Coord current = new Coord(x0, y0, z0);
			// cost for the best known path to each cell
			Dictionary<Coord, int> gscores = new Dictionary<Coord, int>() {{current, 0}};
			// estimated cost to destination cell
			Dictionary<Coord, int> fscores = new Dictionary<Coord, int>() {{current, 0}};
			// cells we know can't be on the path
			HashSet<Coord> closedSet = new HashSet<Coord>();
			// trace for the best known path to each cell
			Dictionary<Coord, Coord> retrace = new Dictionary<Coord, Coord>();
			// queue sorted by fscore of coordinates that need checking
			LinkedList<Coord> queue = new LinkedList<Coord>();
			queue.AddFirst(current);
			// next coordinate to check
			int newScore, cost, fscore;
			bool success = false;
			// check for complete enclosure, which is a common late failure condition
			bool accessible = false;
			foreach (Coord dir in dirs)
			{
                // can I factor out useLast?
				if (useLast && movable(x1+dir.X, y1+dir.Y, z1+dir.Z, x1, y1, z1))
				{
					accessible = true;
				}
				else if (!useLast && standable(x1 + dir.X, y1 + dir.Y, z1 + dir.Z) && condition(x1+dir.X, y1+dir.Y, z1+dir.Z, x1, y1, z1))
				{
					accessible = true;
				}
			}
			if (!accessible)
			{
				return new LinkedList<Coord>();
			}
            //
            int tally = 0;
            int maxtries = 2500;
			while (queue.Count>0) {
                tally += 1;
                if (tally>=maxtries)
                {
                    Debug.Print("failed after {0} tries.",tally);
                    return new LinkedList<Coord>();
                }
				current = queue.First.Value;
				queue.RemoveFirst();
				// ***** if we found the goal, retrace our steps ****
				if (condition(current.X, current.Y, current.Z, x1, y1, z1))
				{
			        success = true;
				}
				if (success)
				{
                    //Debug.Print("Found path after {0} loops.", tally);
					LinkedList<Coord> path = new LinkedList<Coord> {};
					path.AddFirst(current);
					// ***trace backwards
					Coord previous = current;
					while (retrace.ContainsKey(current))
					{		
						previous = retrace[current];
						
					    if (retrace.ContainsKey(previous)) {
					       	current = previous;
						} else {
//							if (useFirst) return previous
							//return current;
							return path;
						}
						path.AddFirst(current);
					}
//					return current;
					return path;
				}
				// ***************************************************
				Coord neighbor;
				LinkedListNode<Coord> node;
				// ***** check passability and scores for neighbors **
				foreach (Coord dir in dirs)
				{
					neighbor = new Coord(current.X+dir.X, current.Y + dir.Y, current.Z + dir.Z);
					if (!closedSet.Contains(neighbor)) {
						if (movable(current.X, current.Y, current.Z, neighbor.X, neighbor.Y, neighbor.Z))
						{
							// cost of taking this step
							cost = 1;
							// actual score along this path	
							// !!! somehow I ran into a situation where this key was missing?							
							newScore = gscores[current] + cost;
							// if this is the best known path to the cell...
							if (!gscores.ContainsKey(neighbor) || gscores[neighbor] > newScore) {
								fscore = newScore + (int) Math.Ceiling(heuristic(neighbor.X, neighbor.Y, neighbor.Z, x1, y1, z1));
								fscores[neighbor] = fscore;
								// loop through the queue to insert in sorted order
								node = queue.First;
								
								// somehow in here it's possible to add the node without setting the gscore?
								if (node==null) {
									queue.AddFirst(neighbor);
									
								} else while (node!=null)
								{
									if (fscores[node.Value]>fscore)
									{
										queue.AddBefore(node, neighbor);
										node = null;
									} else {
										node = node.Next;
										// if we made it all the way to the end, insert it there
										if (node==null)
										{
											queue.AddLast(neighbor);
										}
									}

								}
								retrace[neighbor] = current;
								gscores[neighbor] = newScore;
							}
						} else if (!standable(x1,y1,z1))
						{
							// is the cell imstandable regardless of where we are moving from?
							closedSet.Add(neighbor);
						}
					}
				}
				// ***************************************************
			}
			// ***** failed to find a path ***************************
			Debug.WriteLine("failed to find a path from {0} {1} {2} to {3} {4} {5}",x0,y0,z0,x1,y1,z1);
//			return null;
			return new LinkedList<Coord>();
		}
		
		public static double QuickDistance(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			return Math.Sqrt((x0-x1)*(x0-x1) + (y0-y1)*(y0-y1) + (z0-z1)*(z0-z1));
		}

        public static double QuickDistance(TileEntity t, int x1, int y1, int z1)
        {
            return QuickDistance(t.X, t.Y, t.Z, x1, y1, z1);
        }

        public static double QuickDistance(int x0, int y0, int z0, TileEntity t)
        {
            return QuickDistance(x0, y0, z0, t.X, t.Y, t.Z);
        }

        public static double QuickDistance(TileEntity t0, TileEntity t1)
        {
            return QuickDistance(t0.X, t0.Y, t0.Z, t1.X, t1.Y, t1.Z);
        }


        public static List<Coord> GetLine(int x0, int y0, int x1, int y1)
		{
			return GetLine2D(x0, y0, x1, y1);
		}
		// Bresenham's 2D line algorithm
		public static List<Coord> GetLine2D(int x0, int y0, int x1, int y1)
		{
			List<Coord> line = new List<Coord>();
			int z = -1;
			int dx = Math.Abs(x1-x0);
			int dy = Math.Abs(y1-y0);
			int sx = (x1 > x0) ? 1 : -1;
			int sy = (y1 > y0) ? 1 : -1;
			int err = dx-dy;
			do
			{
				line.Add(new Coord(x0, y0, z));
				int e2 = err*2;
				if (e2>-dy)
				{
					err-=dy;
					x0+=sx;
				}
				if (e2<dx)
				{
					err+=dx;
					y0+=sy;
				}
			} while (x0!=x1 || y0!=y1);
		    return line;
		}
		
		private static List<Coord> getNeighbors(int x, int y, int z, Coord[] c)
		{
			List<Coord> l = new List<Coord>();
			foreach (Coord d in c)
			{
				l.Add(new Coord(d.X+x, d.Y+y, d.Z+z));
			}
			return l;
		}
		
		public static List<Coord> GetNeighbors4(int x, int y, int z)
		{
			return getNeighbors(x, y, z, Movement.Directions4);
		}
		
		public static List<Coord> GetNeighbors8(int x, int y, int z)
		{
			return getNeighbors(x, y, z, Movement.Directions8);
		}
		
		public static List<Coord> GetNeighbors10(int x, int y, int z)
		{
			return getNeighbors(x, y, z, Movement.Directions10);
		}
		
		public static List<Coord> GetNeighbors26(int x, int y, int z)
		{
			return getNeighbors(x, y, z, Movement.Directions26);
		}
		
       

        public static Coord ToCamera(int x, int y)
		{
			Camera Camera = Game.Camera;
			return new Coord(x-Camera.XOffset, y-Camera.YOffset, Camera.Z);
		}
		public static Coord ToCamera(Coord c)
		{
			Camera Camera = Game.Camera;
			return new Coord(c.X-Camera.XOffset, c.Y-Camera.YOffset, Camera.Z);
		}
		public static Coord ToAbsolute(int x, int y)
		{
			Camera Camera = Game.Camera;
			return new Coord(x+Camera.XOffset, y+Camera.YOffset, Camera.Z);
		}
		public static Coord ToAbsolute(Coord c)
		{
			Camera Camera = Game.Camera;
			return new Coord(c.X+Camera.XOffset, c.Y+Camera.YOffset, Camera.Z);
		}
	}	
}