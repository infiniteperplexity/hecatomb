/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 11:33 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Movement.
	/// </summary>
	/// 
	public struct Coord {
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
            int w = OldGame.World.Width;
            int h = OldGame.World.Height;
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
 
        public Coord Rotate(int n)
		{
			if (n==0)
			{
				return this;
			}
			else
			{
				Coord c = new Coord(-Y, X, Z);
				return c.Rotate(n-1);
			}
		}
		
		public Coord Rotate()
		{
			return this.Rotate(1);
		}
		
		public Coord Flip()
		{
			return new Coord(X, Y, -Z);
		}

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public static int Numberize(int x, int y, int z)
        {
            int w = OldGame.World.Width;
            int h = OldGame.World.Height;
            return h * w * z + h * x + y;
        }

        public int Numberize()
        {
            return Numberize(X, Y, Z);
        }

        public int OwnSeed()
        {
            return X * OldGame.World.Width * OldGame.World.Height + Y * OldGame.World.Height + Z + OldGame.World.Turns.Turn;
        }

        public bool Equals(Coord c)
            => X == c.X && Y == c.Y && Z == c.Z;
    }

    public class Movement : Component
    {
        public bool Walks;
        public bool Climbs;
        public bool Flies;
        public bool Swims;
        public bool Phases;
        // used to avoid performance bottlenecks
        [JsonIgnore]
        Actor cachedActor;
        [JsonIgnore]
        Actor CachedActor
        {
            get
            {
                if (cachedActor == null)
                {
                    cachedActor = Entity.Unbox().GetComponent<Actor>();
                }
                return cachedActor;
            }
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
        static Movement()
        {
            // Use the pattern templates in Fallback to populate fallbacks for all directions
            foreach (Coord dir in Directions26)
            {
                if (Fallbacks.ContainsKey(dir))
                {
                    continue;
                }
                else
                {
                    Coord c = dir;
                    int rotate = 0;
                    bool flip = false;
                    bool z = (c.Z != 0);
                    bool diag = (Math.Abs(c.X) + Math.Abs(c.Y) == 2);
                    if (!z && !diag)
                    {
                        while (!c.Equals(North))
                        {
                            rotate += 1;
                            c = c.Rotate();
                        }
                        rotate = 4 - rotate;
                    }
                    else if (!z && diag)
                    {
                        while (!c.Equals(NorthEast))
                        {
                            rotate += 1;
                            c = c.Rotate();
                        }
                        rotate = 4 - rotate;
                    }
                    else if (dir.Equals(Down))
                    {
                        flip = true;
                        c = Up;
                    }
                    else if (z && !diag)
                    {
                        if (c.Z != UpNorth.Z)
                        {
                            c.Z = UpNorth.Z;
                            flip = true;
                        }
                        while (!c.Equals(UpNorth))
                        {
                            rotate += 1;
                            c = c.Rotate();
                        }
                        rotate = 4 - rotate;
                    }
                    else if (z && diag)
                    {
                        if (c.Z != UpNorthEast.Z)
                        {
                            c.Z = UpNorthEast.Z;
                            flip = true;
                        }
                        while (!c.Equals(UpNorthEast))
                        {
                            rotate += 1;
                            c = c.Rotate();
                        }
                        rotate = 4 - rotate;
                    }
                    var f = Fallbacks[c];
                    var g = new Coord[f.Length][];
                    for (int i = 0; i < f.Length; i++)
                    {
                        g[i] = new Coord[f[i].Length];
                        for (int j = 0; j < f[i].Length; j++)
                        {
                            g[i][j] = (flip) ? f[i][j].Rotate(rotate).Flip() : f[i][j].Rotate(rotate);
                        }
                    }
                    Fallbacks[dir] = g;
                }
            }
        }

        public Movement() : base()
        {
            Walks = true;
            Climbs = true;
            Flies = false;
            Swims = true;
            Phases = false;
            Required = new string[] { "Actor" };
        }

        // was there a version of this that can push the creature?
        public void Displace(Creature c)
        {
            if (c == this.Entity)
            {
                 throw new InvalidOperationException("We're trying to displace ourselves, that's totally illegal.");
            }
            Displace(c, Entity.X, Entity.Y, Entity.Z);
        }
        public void Displace(Creature cr, int x, int y, int z)
        {
            int x1 = cr.X;
            int y1 = cr.Y;
            int z1 = cr.Z;
            cr.Remove();
            if (OldGame.World.Creatures[x1, y1, z1] != null)
            {
                Debug.WriteLine("how on earth did this happen?");
            }
            StepTo(x1, y1, z1);
            Movement m = cr.TryComponent<Movement>();
            if (m != null)
            {
                m.StepTo(x, y, z);
            }
            else
            {
                cr.Place(x, y, z);
            }
        }

        public bool CanStand(int x1, int y1, int z1)
        {
            if (x1 < 0 || x1 >= OldGame.World.Width || y1 < 0 || y1 >= OldGame.World.Height || z1 < 0 || z1 >= OldGame.World.Depth) {
                return false;
            }
            Terrain tile = OldGame.World.Terrains[x1, y1, z1];
            // non-phasers can't go through a solid wall
            if (!Phases && tile.Solid) {
                return false;
            }
            // non-flyers can't cross a pit
            if (tile.Fallable && Flies == false) {
                return false;
            }
            if (CachedActor.Team == Teams.Friendly)
            {
                Task t = OldGame.World.Tasks[x1, y1, z1];
                if (t != null && t is ForbidTask)
                {
                    return false;
                }
            }
            if (CachedActor.Team != Teams.Friendly)
            {
                // doors block non-allied creatures
                Feature f = OldGame.World.Features[x1, y1, z1];
                if (f != null && f.Solid && !Phases)
                {
                    return false;
                }
            }
            int dx = x1 - Entity.X;
            int dy = y1 - Entity.Y;
            int dz = z1 - Entity.Z;
            // rare: check whether the square itself is allowed
            if (dx == 0 && dy == 0 && dz == 0)
            {
                return true;
            }
            // check for liquid some day....
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

        public bool CouldMove(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            if (!CanStand(x1, y1, z1))
            {
                return false;
            }
            int dx = x1 - x0;
            int dy = y1 - y0;
            int dz = z1 - z0;
            // rare: check whether the square itself is allowed
            if (dx == 0 && dy == 0 && dz == 0)
            {
                return true;
            }
            // non-flyers can't climb diagonally
            if (!Flies && dz != 0 && (dx != 0 || dy != 0))
            {
                return false;
            }
            // non-flyers need a slope in order to go up
            Terrain t0 = OldGame.World.Terrains[x0, y0, z0];
            if (dz == +1 && !Flies && t0.ZWalk != +1)
            {
                return false;
            }
            Terrain tile = OldGame.World.Terrains[x1, y1, z1];
            // non-phasers can't go through a ceiling
            if (!Phases)
            {
                if (dz == +1 && !tile.Fallable && tile.ZWalk != -1)
                {
                    return false;
                }
                else if (dz == -1 && t0.ZWalk != -1 && !t0.Fallable)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanMove(int x1, int y1, int z1)
        {
            return CouldMove(Entity.X, Entity.Y, Entity.Z, x1, y1, z1);
        }

        public bool CanPass(int x1, int y1, int z1)
        {
            if (!CanMove(x1, y1, z1))
            {
                return false;
            }
            if (OldGame.World.Creatures[x1, y1, z1] != null)
            {
                return false;
            }
            return true;
        }

        public float MovementCost(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            //Feature f = Game.World.Features[x1, y1, z1];
            //if (f != null && f.Solid && CachedActor.Team != Teams.Friendly)
            //{
            //    return 12;
            //}
            Cover cv = OldGame.World.Covers[x1, y1, z1];
            if (cv.Liquid)
            {
                return 2;
            }
            Terrain t = OldGame.World.Terrains[x1, y1, z1];
            if (t == Terrain.UpSlopeTile || t == Terrain.DownSlopeTile)
            {
                return 2;
            }
            if (z1 > z0)
            {
                return 3;
            }
            return 1;
        }
				
		public void StepTo(int x1, int y1, int z1)
		{
			Entity.Entity.Place(x1, y1, z1);
            //Actor a = Entity.GetComponent<Actor>();
            //a.Spend(16);
            CachedActor.Spend(16);
		}

        // tentative...this does not allow (1) diagonal work/attacks or (2) digging upward...could handle the latter with ramps
        public bool CouldTouch(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int dz = z1 - z0;
            int dx = x1 - x0;
            int dy = y1 - y0;
            if (dz==0)
            {
               if (Math.Abs(dx)<=1 && Math.Abs(dy)<=1)
                {
                    return true;
                }
            }
            else if (dz==+1 && dx==0 && dy==0 && OldGame.World.Terrains[x1, y1, z1].ZView == -1)
            {
                return true;
            }
            else if (dz==-1 && dx == 0 && dy == 0 && OldGame.World.Terrains[x0, y0, z0].ZView == -1)
            {
                return true;
            }
            return false;
        }

        public bool CanTouch(int x1, int y1, int z1)
        {
            return CouldTouch(Entity.X, Entity.Y, Entity.Z, x1, y1, z1);
        }
		
		
		
		public Func<int, int, int, bool> DelegateCanMove()
		{
			return (int x, int y, int z)=>{return CanMove(x, y, z);};
		}
		public Func<int, int, int, bool> DelegateCanStand()
		{
			return (int x, int y, int z)=>{return CanStand(x, y, z);};
		}
		
		public bool CanReach(int x1, int y1, int z1, bool useLast=true)
		{
			int x0 = Entity.X;
			int y0 = Entity.Y;
			int z0 = Entity.Z;
            Func<int, int, int, int, int, int, bool> movable;
            Func<int, int, int, bool> standable;
            movable = CouldMove;
            standable = CanStand;
			var path = Tiles.FindPath(this, x1, y1, z1, useLast: useLast, movable: movable, standable: standable);
			Coord? c = (path.Count>0) ? path.First.Value : (Coord?) null;		
			return (c==null) ? false : true;
		}


        public bool CanReach(TileEntity t, bool useLast = true, bool useCache = true)
        {
            Func<int, int, int, int, int, int, bool> movable;
            Func<int, int, int, bool> standable;
            movable = CouldMove;
            standable = CanStand;
            var path = Tiles.FindPath(this, t, useLast: useLast, movable: movable, standable: standable, useCache: useCache);
            Coord? c = (path.Count > 0) ? path.First.Value : (Coord?)null;
            return (c == null) ? false : true;
        }

        public bool CanFindResources(Dictionary<string, int> resources, bool respectClaims = true, bool ownedOnly = true, bool alwaysNeedsIngredients = false, bool useCache = true)
        {
            if (OldGame.Options.NoIngredients && !alwaysNeedsIngredients)
            {
                return true;
            }
            Dictionary<string, int> needed = new Dictionary<string, int>(resources);
            List<Item> items = OldGame.World.Items.Where(
                it => { return (needed.ContainsKey(it.Resource) && (ownedOnly == false || it.Owned) && (!respectClaims || it.Unclaimed > 0) && CanReach(it, useCache: useCache)); }
            ).ToList();
            foreach (Item item in items)
            {
                if (needed.ContainsKey(item.Resource))
                {
                    int n = (respectClaims) ? item.Unclaimed : item.Quantity;
                    needed[item.Resource] -= n;
                    if (needed[item.Resource] <= 0)
                    {
                        needed.Remove(item.Resource);
                    }
                }
            }
            return (needed.Count == 0);
        }

        public bool CanFindResource(string resource, int need, bool respectClaims = true, bool ownedOnly = true, bool useCache = true)
        {
            if (OldGame.Options.NoIngredients)
            {
                return true;
            }
            List<Item> items = OldGame.World.Items.Where(
                it => { return (it.Resource == resource && (ownedOnly == false || it.Owned) && (!respectClaims || it.Unclaimed > 0) && CanReach(it, useCache: useCache)); }
            ).ToList();
            int needed = need;
            foreach (Item item in items)
            {
                int n = (respectClaims) ? item.Unclaimed : item.Quantity;
                needed -= n;
            }
            return (needed <= 0);
        }
    }
}
