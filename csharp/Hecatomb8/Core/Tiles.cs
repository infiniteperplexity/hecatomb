using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb8
{
    //using static HecatombAliases;
    // Tiles is a grouping of convenience, for static methods involving grid geometry
    public static partial class Tiles
    {
        public static double Distance(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            return Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1) + (z0 - z1) * (z0 - z1));
        }

        // Bresenham's 2D line algorithm
        public static List<Coord> GetLine(int x0, int y0, int x1, int y1)
        {
            List<Coord> line = new List<Coord>();
            int z = -1;
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x1 > x0) ? 1 : -1;
            int sy = (y1 > y0) ? 1 : -1;
            int err = dx - dy;
            do
            {
                line.Add(new Coord(x0, y0, z));
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            } while (x0 != x1 || y0 != y1);
            return line;
        }

        private static List<Coord> getNeighbors(int x, int y, int z, Coord[] c, Func<int, int, int, bool>? where = null)
        {
            where = where ?? ((int xx, int yy, int zz) => (true));
            List<Coord> l = new List<Coord>();
            foreach (Coord d in c)
            {
                if (x < 0 || x > GameState.World!.Width - 1 || y < 0 || y > GameState.World!.Height - 1 || z < 0 || z > GameState.World!.Depth - 1)
                {
                    continue;
                }
                if (where(d.X + x, d.Y + y, d.Z + z))
                {
                    l.Add(new Coord(d.X + x, d.Y + y, d.Z + z));
                }
            }
            return l;
        }
        public static List<Coord> GetNeighbors4(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions4, where: where);
        }

        public static List<Coord> GetNeighbors8(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions8, where: where);
        }

        public static List<Coord> GetNeighbors6(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions6, where: where);
        }

        public static List<Coord> GetNeighbors10(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions10, where: where);
        }

        public static List<Coord> GetNeighbors26(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions26, where: where);
        }
    }
}