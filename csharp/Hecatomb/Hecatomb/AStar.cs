/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 3:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of AStar.
	/// </summary>
	public static class Tiles
	{
		
		private static bool defaultPassable(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH)
			{
      			return false;
    		}
			Terrain t = Game.World.Tiles[x1,y1,z1];
			return (!t.Solid && !t.Fallable);
		}
		
		public static Coord? FindPath(int x0, int y0, int z0, int x1, int y1, int z1,
			Func<int, int, int, int, int, int, int> heuristic = null,
			Func<int, int, int, int, int, int, bool> passable = null)
		{
			// default value for the cost estimation heuristic
			heuristic = heuristic ?? QuickDistance;
			// default value for allowable movement
			passable = passable ?? defaultPassable;
			// !should check enclosed right up front
			// !this doesn't have to be shuffled but it would be nice if it were
			Coord[] dirs = Movement.Directions26;
			//Coord[] dirs = Game.Random.Shuffled(Movement.Directions26);
			// score for the best known path to each cell
			Dictionary<Coord, int> gscore = new Dictionary<Coord, int>();
			// trace for the best known path to each cell
			Dictionary<Coord, Coord> retrace = new Dictionary<Coord, Coord>();
			// queue sorted by fscore of coordinates that need checking
			SortedList<int, Coord> queue = new SortedList<int, Coord>() {{0, new Coord(x0, y0, z0)}};
			// next coordinate to check
			Coord current;
			int newScore, cost, fscore;
			while (queue.Count>0) {
				current = queue[0];
				queue.RemoveAt(0);
				// ***** if we found the goal, retrace our steps ****
				if (current.x==x1 && current.y==y1 && current.z==z1) {
					// ***trace backwards
					Coord previous = current;
					while (retrace.ContainsKey(current))
					{
					       	previous = retrace[current];
					       	if (retrace.ContainsKey(previous)) {
					       		current = previous;
					       	}
					}
					return current;
				}
				// ***************************************************
				Coord neighbor;
				// ***** check passability and scores for neighbors **
				foreach (Coord dir in dirs)
				{
					neighbor = new Coord(current.x+dir.x, current.y + dir.y, current.z + dir.z);
					if (passable(current.x, current.y, current.z, neighbor.x, neighbor.y, neighbor.z))
					{
						cost = 1;
						newScore = gscore[current] + cost;
						if (!gscore.ContainsKey(neighbor) || gscore[neighbor] > newScore) {
							fscore = newScore + heuristic(neighbor.x, neighbor.y, neighbor.z, x1, y1, z1);
							queue.Add(fscore, neighbor);
							retrace[neighbor] = current;
							gscore[neighbor] = newScore;
						}
					}
				}
				// ***************************************************
			}
			// ***** failed to find a path ***************************
			return null;
		}
		
		public static int QuickDistance(int x0, int y0, int z0, int x1, int y1, int z1)
		{
			return Math.Abs(x0-x1) + Math.Abs(y0-y1) + Math.Abs(z0-z1);
		}
	}	
}