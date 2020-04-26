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

        public Coord(int n)
        {
            int m = n;
            int w = GameState.World!.Width;
            int h = GameState.World!.Height;
            X = 0;
            Y = 0;
            Z = 0;
            while (m - w * h >= 0)
            {
                m -= w * h;
                Z += 1;
            }
            while (m - h >= 0)
            {
                m -= h;
                X += 1;
            }
            Y = m;
        }

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public static int Numberize(int x, int y, int z)
        {
            int w = GameState.World!.Width;
            int h = GameState.World!.Height;
            return h * w * z + h * x + y;
        }

        public int Numberize()
        {
            return Numberize(X, Y, Z);
        }

        public int OwnSeed()
        {
            return X * GameState.World!.Width * GameState.World!.Height + Y * GameState.World!.Height + Z + GameState.World!.GetState<TurnHandler>().Turn;
        }

        public bool Equals(Coord c)
            => X == c.X && Y == c.Y && Z == c.Z;

        //public override bool Equals(Object o)
        //{
        //    if (o is Coord)
        //    {
        //        Coord c = (Coord)o;
        //        return (X == c.X && Y == c.Y && Z == c.Z);
        //    }
        //    return false;
        //}
            

        public static bool operator ==(Coord one, Coord two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(Coord one, Coord two)
        {
            return !one.Equals(two);
        }

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
    }

}
