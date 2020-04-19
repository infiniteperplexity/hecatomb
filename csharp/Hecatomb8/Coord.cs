using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    // Coord is a value type that represents a 3-dimensional coordinate.  It does not use Constrained<int> although I could maybe add a conversion
    // I could also potentially add some kind of conversions for nullable int?.
    public struct Coord
    {
        public int X;
        public int Y;
        public int Z;

        public Coord(int _x, int _y, int _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public bool Equals(Coord c)
            => X == c.X && Y == c.Y && Z == c.Z;

        public readonly static Coord North = new Coord(+0, -1, +0);
        public readonly static Coord South = new Coord(+0, +1, +0);
        public readonly static Coord East = new Coord(+1, +0, +0);
        public readonly static Coord West = new Coord(-1, +0, +0);
        public readonly static Coord NorthEast = new Coord(+1, -1, +0);
        public readonly static Coord SouthEast = new Coord(+1, +1, +0);
        public readonly static Coord NorthWest = new Coord(-1, -1, +0);
        public readonly static Coord SouthWest = new Coord(-1, +1, +0);
        public readonly static Coord UpNorth = new Coord(+0, +1, +1);
        public readonly static Coord UpSouth = new Coord(+0, -1, +1);
        public readonly static Coord UpEast = new Coord(+1, +0, +1);
        public readonly static Coord UpWest = new Coord(-1, +0, +1);
        public readonly static Coord UpNorthEast = new Coord(+1, -1, +1);
        public readonly static Coord UpSouthEast = new Coord(+1, +1, +1);
        public readonly static Coord UpNorthWest = new Coord(-1, -1, +1);
        public readonly static Coord UpSouthWest = new Coord(-1, +1, +1);
        public readonly static Coord DownNorth = new Coord(+0, -1, -1);
        public readonly static Coord DownSouth = new Coord(+0, +1, -1);
        public readonly static Coord DownEast = new Coord(+1, +0, -1);
        public readonly static Coord DownWest = new Coord(-1, +0, -1);
        public readonly static Coord DownNorthEast = new Coord(+1, -1, -1);
        public readonly static Coord DownSouthEast = new Coord(+1, +1, -1);
        public readonly static Coord DownNorthWest = new Coord(-1, -1, -1);
        public readonly static Coord DownSouthWest = new Coord(-1, +1, -1);
        public readonly static Coord Up = new Coord(+0, +0, +1);
        public readonly static Coord Down = new Coord(+0, +0, -1);

        public static Coord[] Directions4 = new Coord[] {
            North,
            South,
            East,
            West
        };

        public static Coord[] Directions6 = new Coord[] {
            North,
            South,
            East,
            West,
            Up,
            Down
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
            East,
            South,
            West,
            NorthEast,
            SouthEast,
            SouthWest,
            NorthWest,
            UpNorth,
            UpEast,
            UpSouth,
            UpWest,
            DownNorth,
            DownEast,
            DownSouth,
            DownWest,
            Up,
            Down,
            UpNorthEast,
            UpSouthEast,
            UpSouthWest,
            UpNorthWest,
            DownNorthEast,
            DownSouthEast,
            DownSouthWest,
            DownNorthWest
        };
        // this next structure describes the "fallback" directions for movement when a creature tries and fails to move in a certain direction
        // it really belongs in Movement but namespacing it there would be really annoying
        public static Dictionary<Coord, Coord[][]> Fallbacks = new Dictionary<Coord, Coord[][]> {
            {North, new Coord[][]
            {
                new [] {North},
                new [] {UpNorth, DownNorth},
                new [] {NorthEast, NorthWest},
                new [] {UpNorthEast, UpNorthWest, DownNorthEast, DownNorthWest},
                new [] {Up, Down},
                new [] {East, West, UpEast, UpWest, DownEast, DownWest},
                new [] {SouthEast, SouthWest, UpSouthEast, UpSouthWest, DownSouthEast, DownSouthWest},
                new [] {UpSouth , DownSouth},
                new [] {South}
            }},
            {NorthEast, new Coord[][]
            {
                new [] {NorthEast},
                new [] {UpNorthEast, DownNorthEast},
                new [] {Up, Down},
                new [] {North, East, UpNorth, UpEast, DownNorth, DownEast},
                new [] {UpNorthWest, DownNorthWest, UpSouthEast, UpSouthWest, NorthWest, SouthEast},
                new [] {West, South, UpWest, DownWest, UpSouth, DownSouth},
                new [] {UpSouthWest, DownSouthWest},
                new [] {SouthWest}
            }},
            {UpNorth, new Coord[][]
            {
                new [] {UpNorth},
                new [] {UpNorthWest, UpNorthEast},
                new [] {NorthWest, NorthEast, Up, UpEast, UpWest},
                new [] {East, West},
                new [] {DownNorth, DownNorthWest, DownNorthEast, UpSouthWest, UpSouthEast},
                new [] {DownEast, DownWest, SouthEast, SouthWest, UpSouth, Down, South},
                new [] {DownSouthEast, DownSouthWest},
                new [] {DownSouth}
            }},
            {Up, new Coord[][]
            {
                new [] {Up},
                new [] {UpNorth, UpSouth, UpEast, UpWest, UpNorthEast, UpNorthWest, UpSouthWest, UpSouthEast},
                new [] {East, West, North, South, NorthEast, SouthWest, NorthWest, SouthEast},
                new [] {DownNorth, DownSouth, DownEast, DownWest, DownSouthEast, DownSouthWest, DownNorthEast, DownNorthWest},
                new [] {Down}
            }},
            {UpNorthEast, new Coord[][]
            {
                new [] {UpNorthEast},
                new [] {UpNorth, UpEast, NorthEast},
                new [] {UpSouthEast, UpNorthWest, Up, North, East},
                new [] {SouthEast, NorthWest, DownNorthEast},
                new [] {DownNorth, DownEast, UpSouth, UpWest},
                new [] {DownSouthEast, DownNorthWest, Down, South, West, UpSouthWest},
                new [] {DownSouth, DownWest, SouthWest},
                new [] {DownSouthWest}
            }}
        };
    }

}
