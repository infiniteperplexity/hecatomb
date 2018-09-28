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
		
		private static bool defaultPassable(int x1, int y1, int z1)
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
		
		public static Coord? FindPath(int x0, int y0, int z0, int x1, int y1, int z1,
			Func<int, int, int, int, int, int, double> heuristic = null,
			Func<int, int, int, int, int, int, bool> movable = null,
			Func<int, int, int, bool> passable = null
		) {
//			Debug.WriteLine("Finding path from {0} {1} {2} to {3} {4} {5}",x0,y0,z0,x1,y1,z1);
			// default value for the cost estimation heuristic
			heuristic = heuristic ?? QuickDistance;
			// default value for allowable movement
			movable = movable ?? defaultMovable;
			passable = passable ?? defaultPassable;
			// !should check enclosed right up front
			// !this doesn't have to be shuffled but it would be nice if it were
			Coord[] dirs = Movement.Directions26;
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
			while (queue.Count>0) {
				current = queue.First.Value;
				queue.RemoveFirst();
				// ***** if we found the goal, retrace our steps ****
				if (current.x==x1 && current.y==y1 && current.z==z1) {
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
					neighbor = new Coord(current.x+dir.x, current.y + dir.y, current.z + dir.z);
					if (!closedSet.Contains(neighbor)) {
						if (movable(current.x, current.y, current.z, neighbor.x, neighbor.y, neighbor.z))
						{
							// cost of taking this step
							cost = 1;
							// actual score along this path	
							// !!! somehow I ran into a situation where this key was missing?							
							newScore = gscores[current] + cost;
							// if this is the best known path to the cell...
							if (!gscores.ContainsKey(neighbor) || gscores[neighbor] > newScore) {
								fscore = newScore + (int) Math.Ceiling(heuristic(neighbor.x, neighbor.y, neighbor.z, x1, y1, z1));
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
						} else if (!passable(x1,y1,z1))
						{
							// is the cell impassable regardless of where we are moving from?
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
	}	
}