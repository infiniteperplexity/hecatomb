﻿/*
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

namespace Hecatomb
{
	/// <summary>
	/// Description of AStar.
	public static class Tiles
	{
		
		private static bool defaultStandable(int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH)
			{
      			return false;
    		}
			Terrain t = Game.World.Tiles[x1,y1,z1];
			return (!t.Solid && !t.Fallable);
		}
		
		private static bool defaultMovable(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH)
			{
      			return false;
    		}
			Terrain t = Game.World.Tiles[x1,y1,z1];
			return (!t.Solid && !t.Fallable);
		}
		
		private static Dictionary<int, Dictionary<int, int>> pathHits = new Dictionary<int, Dictionary<int, int>>();
		private static Dictionary<int, Dictionary<int, int>> pathMisses = new Dictionary<int, Dictionary<int, int>>();
		
		public static Coord? FindPath(Movement m, TypedEntity t, bool useLast=true)
		{
			return null;
		}
		public static Coord? FindPath(Movement m, int x1, int y1, int z1, bool useLast = true)
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
			return FindPath(
				x0, y0, z0, x1, y1, z1,
				useLast: useLast,
				movable: movable,
				standable: standable
			);
		}
		public static Coord? FindPath(int x0, int y0, int z0, int x1, int y1, int z1,
		    bool useLast = true,
		    bool useFirst = false,
			Func<int, int, int, int, int, int, double> heuristic = null,
			Func<int, int, int, int, int, int, bool> movable = null,
			Func<int, int, int, bool> standable = null
		) {
//			Debug.WriteLine("Finding path from {0} {1} {2} to {3} {4} {5}",x0,y0,z0,x1,y1,z1);
			// default value for the cost estimation heuristic
			heuristic = heuristic ?? QuickDistance;
			// default value for allowable movement
			movable = movable ?? defaultMovable;
			standable = standable ?? defaultStandable;
			// !should check enclosed right up front
			// !this doesn't have to be shuffled but it would be nice if it were
			Coord[] dirs = Movement.Directions10;
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
			while (queue.Count>0) {
				current = queue.First.Value;
				queue.RemoveFirst();
				// ***** if we found the goal, retrace our steps ****
				if (current.X==x1 && current.Y==y1 && current.Z==z1)
				{
					success = true;
				}
				else if (!useLast && QuickDistance(current.X, current.Y, current.Z, x1, y1, z1)<=1)
				{
					success = true;
				}
				if (success)
				{
					// ***trace backwards
					Coord previous = current;
					while (retrace.ContainsKey(current))
					{		
						previous = retrace[current];
					    if (retrace.ContainsKey(previous)) {
					       	current = previous;
						} else {
//							if (useFirst) return previous
							return current;
						}
					}
					return current;
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
			return null;
		}
		
		public static double QuickDistance(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			return Math.Sqrt((x0-x1)*(x0-x1) + (y0-y1)*(y0-y1) + (z0-z1)*(z0-z1));
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
		
		
		public static char GetSymbol(int x, int y, int z)
		{
			Creature cr = Game.World.Creatures[x,y,z];
			Feature fr = Game.World.Features[x,y,z];
			Terrain t = Game.World.Tiles[x,y,z];
			Item it = Game.World.Items[x, y, z];
			List<Particle> pl = Game.MainPanel.Particles[x,y,z];
			Particle p = (pl.Count>0) ? pl[0] : null;
			var c = new Coord(x, y, z);
			if (p!=null && p.Symbol!=default(char))
			{
				return p.Symbol;
			}
			if (!Game.World.Explored.Contains(c))
			{
				return ' ';
			}
			else if (!Game.Visible.Contains(c))
			{
				if (it!=null)
				{
					return it.Symbol;
				}
				else if (fr!=null)
				{
					return fr.Symbol;
				}
				else
				{
					return t.Symbol;
				}
			}
			else
			{
				if (cr!=null)
				{
					return cr.Symbol;
				}
				else if (it!=null)
				{
					return it.Symbol;
				}
				else if (fr!=null)
				{
					return fr.Symbol;
				}
				else
				{
					return t.Symbol;
				}
			}
		}

		public static string GetFG(int x, int y, int z)
		{
			Creature cr = Game.World.Creatures[x,y,z];
			Feature fr = Game.World.Features[x,y,z];
			Terrain t = Game.World.Tiles[x,y,z];
			Item it = Game.World.Items[x, y, z];
			List<Particle> pl = Game.MainPanel.Particles[x,y,z];
			Particle p = (pl.Count>0) ? pl[0] : null;
			var c = new Coord(x, y, z);
			if (p!=null && p.FG!=null)
			{
				return p.FG;
			}
			if (!Game.World.Explored.Contains(c))
			{
				return "black";
			}
			else if (!Game.Visible.Contains(c))
			{
				return "SHADOWFG";
			}
			else if (cr!=null)
			{
				return cr.FG;
			}
			else if (it!=null)
			{
				return it.FG;
			}
			else if (fr!=null)
			{
				return fr.FG;
			}
			else
			{
				return t.FG;
			}
		}

		public static string GetBG(int x, int y, int z)
		{
			List<Particle> pl = Game.MainPanel.Particles[x,y,z];
			Particle p = (pl.Count>0) ? pl[0] : null;
			Terrain t = Game.World.Tiles[x, y, z];
			Feature f = Game.World.Features[x, y, z];
			Item it = Game.World.Items[x, y, z];
			var c = new Coord(x, y, z);
			TaskEntity task = Game.World.Tasks[x, y, z];
			if (p!=null && p.BG!=null)
			{
				return p.BG;
			}
			if (!Game.World.Explored.Contains(c))
			{
				return "black";
			}
			else if (task!=null)
			{
				return "orange";
			}
			else if (!Game.Visible.Contains(c))
			{
				return "black";
			}
			else if (it!=null && it.BG!=null)
			{
				return it.FG;
			}
			else if (f!=null && f.BG!=null)
			{
				return f.BG;
			}
			else
			{
				return t.BG;
			}
		}
		
		public static Tuple<char, string, string> GetGlyph(int x, int y, int z)
		{
			return new Tuple<char, string, string>(GetSymbol(x, y, z), GetFG(x, y, z), GetBG(x, y, z));
		}
		
		public static Coord ToCamera(int x, int y)
		{
			GameCamera Camera = Game.Camera;
			return new Coord(x-Camera.XOffset, y-Camera.YOffset, Camera.Z);
		}
		public static Coord ToCamera(Coord c)
		{
			GameCamera Camera = Game.Camera;
			return new Coord(c.X-Camera.XOffset, c.Y-Camera.YOffset, Camera.Z);
		}
		public static Coord ToAbsolute(int x, int y)
		{
			GameCamera Camera = Game.Camera;
			return new Coord(x+Camera.XOffset, y+Camera.YOffset, Camera.Z);
		}
		public static Coord ToAbsolute(Coord c)
		{
			GameCamera Camera = Game.Camera;
			return new Coord(c.X+Camera.XOffset, c.Y+Camera.YOffset, Camera.Z);
		}
	}	
}