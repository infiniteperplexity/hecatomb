using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{ 
    //not that this is not an eller maze any more
    public class Maze
    {
        public static bool[,,] Generate(int width, int height)
        {
            var random = Game.World.Random;
            // the last dimension is (right, bottom)
            var maze = new bool[width, height, 2];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    maze[i, j, 0] = true;
                    maze[i, j, 1] = true;
                }
            }
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            int x = random.Next(width);
            int y = random.Next(height);
            visited.Add((x, y));
            int dx = 0;
            int dy = 0;
            int r;
            bool allowed = false;
            while (visited.Count < width * height)
            {
                // randomly choose an adjacent cell
                allowed = false;
                while (!allowed)
                {
                    r = random.Next(4);
                    if (r==0)
                    {
                        dx = 1;
                        dy = 0;
                    }
                    else if (r==1)
                    {
                        dx = -1;
                        dy = 0;
                    }
                    else if (r==2)
                    {
                        dx = 0;
                        dy = 1;
                    }
                    else
                    {
                        dx = 0;
                        dy = -1;
                    }
                    if (x + dx >=0 && x + dx < width && y + dy >= 0 && y + dy < height)
                    {
                        allowed = true;
                    }
                }
                x += dx;
                y += dy;
                if (!visited.Contains((x, y)))
                {
                    if (dx == 1)
                    {
                        maze[x - dx, y, 0] = false;
                    }
                    else if (dx == -1)
                    {
                        maze[x, y, 0] = false;
                    }
                    else if (dy == 1)
                    {
                        maze[x, y - dy, 1] = false;
                    }
                    else if (dy == -1)
                    {
                        maze[x, y, 1] = false;
                    }
                }
                visited.Add((x, y));
            }
            return maze;
        }
    }
}