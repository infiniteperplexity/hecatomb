/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 1:14 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of RecursiveBacktracker.
	/// </summary>
	public class RecursiveBacktracker
	{
		public bool[,] RightWalls;
		public bool[,] BottomWalls;
		public int Height;
		public int Width;
		private Random random;
		private struct XY
		{
			public int X;
			public int Y;
			public XY(int x, int y)
			{
				X = x;
				Y = y;
			}	
		}
		private XY[] dirs;
		private HashSet<XY> visited;
		
		private static XY Up = new XY(+0, -1);
		private static XY Right = new XY(+1, +0);
		private static XY Left = new XY(-1, +0);
		private static XY Down = new XY(+0, +1);
		
		public RecursiveBacktracker(int x, int y, int seed=0)
		{
			random = new Random(seed);
			Width = x;
			Height = y;
			RightWalls = new bool[x,y];
			BottomWalls = new bool[x,y];
			dirs = new XY[] {Up, Down, Left, Right};
			visited = new HashSet<XY>();
			recurse(new XY(random.Next(x), random.Next(y)));
		}
		private void recurse(XY cell)
		{
			dirs = dirs.OrderBy((XY xy)=>random.NextDouble()).ToArray();
			for (int i=0; i<dirs.Length; i++)
			{
				int x = cell.X+dirs[i].X;
				int y = cell.Y+dirs[i].Y;
				if (x>=0 && x<Width && y>=0 && y<Height)
				{
					XY xy = new XY(x, y);
					if (!visited.Contains(xy))
					{
						visited.Add(xy);
						if (dirs[i].Equals(Up))
						{
							BottomWalls[x, y] = true;
						}
						else if (dirs[i].Equals(Down))
						{
							BottomWalls[cell.X, cell.Y] = true;
						}
						else if (dirs[i].Equals(Right))
						{
							RightWalls[cell.X, cell.Y] = true;
						}
						else if (dirs[i].Equals(Left))
						{
							RightWalls[x, y] = true;
						}
						recurse(xy);
					}
				}
			}
		}
	}
}