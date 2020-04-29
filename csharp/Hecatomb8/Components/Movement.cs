using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static Coord;
    using static HecatombAliases;
    public class Movement : Component
    {
        public bool Walks;
        public bool Flies;
        public bool Swims;
        public bool Phases;

        // this next structure describes the "fallback" directions for movement when a creature tries and fails to move in a certain direction
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

        public Movement()
        {
            Walks = true;
        }
        // used to avoid performance bottlenecks
        //[JsonIgnore]
        //Actor cachedActor;
        //[JsonIgnore]
        //Actor CachedActor
        //{
        //    get
        //    {
        //        if (cachedActor == null)
        //        {
        //            cachedActor = Entity.Unbox().GetComponent<Actor>();
        //        }
        //        return cachedActor;
        //    }
        //}

        // was there a version of this that can push the creature?
        //public void Displace(Creature c)
        //{
        //    var e = Entity.UnboxBriefly()!;
        //    if (c == e)
        //    {
        //        throw new InvalidOperationException("We're trying to displace ourselves, that's totally illegal.");
        //    }
        //    Displace(c, e.X, e.Y, e.Z);
        //}
        //public void Displace(Creature cr, int x, int y, int z)
        //{
        //    int x1 = cr.X;
        //    int y1 = cr.Y;
        //    int z1 = cr.Z;
        //    cr.Remove();
        //    if (Game.World.Creatures[x1, y1, z1] != null)
        //    {
        //        Debug.WriteLine("how on earth did this happen?");
        //    }
        //    StepTo(x1, y1, z1);
        //    Movement m = cr.TryComponent<Movement>();
        //    if (m != null)
        //    {
        //        m.StepTo(x, y, z);
        //    }
        //    else
        //    {
        //        cr.Place(x, y, z);
        //    }
        //}

        public bool CanStandBounded(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            if (x1 < 0 || x1 >= GameState.World!.Width || y1 < 0 || y1 >= GameState.World!.Height || z1 < 0 || z1 >= GameState.World!.Depth)
            {
                return false;
            }
            Terrain tile = GameState.World!.Terrains.GetWithBoundsChecked(x1, y1, z1);
            // non-phasers can't go through a solid wall
            if (!Phases && tile.Solid)
            {
                return false;
            }
            // non-flyers can't cross a pit
            if (tile.Fallable && Flies == false)
            {
                return false;
            }
            //if (CachedActor.Team == Teams.Friendly)
            //{
            //    Task t = Game.World.Tasks[x1, y1, z1];
            //    if (t != null && t is ForbidTask)
            //    {
            //        return false;
            //    }
            //}
            //if (CachedActor.Team != Teams.Friendly)
            //{
            //    // doors block non-allied creatures
            //    Feature f = Game.World.Features[x1, y1, z1];
            //    if (f != null && f.Solid && !Phases)
            //    {
            //        return false;
            //    }
            //}
            var e = Entity.UnboxBriefly()!;
            int dx = x1 - (int)e!.X!;
            int dy = y1 - (int)e!.Y!;
            int dz = z1 - (int)e!.Z!;
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

        public bool CouldMoveBounded(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            if (!CanStandBounded(x1, y1, z1))
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
            Terrain t0 = GameState.World!.Terrains.GetWithBoundsChecked(x0, y0, z0);
            if (dz == +1 && !Flies && t0.Slope != +1)
            {
                return false;
            }
            Terrain tile = GameState.World.Terrains.GetWithBoundsChecked(x1, y1, z1);
            // non-phasers can't go through a ceiling
            if (!Phases)
            {
                if (dz == +1 && !tile.Fallable && tile.Slope != -1)
                {
                    return false;
                }
                else if (dz == -1 && t0.Slope != -1 && !t0.Fallable)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanMoveBounded(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            var e = Entity.UnboxBriefly()!;
            return CouldMoveBounded((int)e.X!, (int)e.Y!, (int)e.Z!, x1, y1, z1);
        }

        public bool CanPassBounded(int x1, int y1, int z1)
        {
            if (!CanMoveBounded(x1, y1, z1))
            {
                return false;
            }
            if (GameState.World!.Creatures.GetWithBoundsChecked(x1, y1, z1) != null)
            {
                return false;
            }
            return true;
        }

        public float GetMoveCostBounded(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            Cover cv = GameState.World!.Covers.GetWithBoundsChecked(x1, y1, z1);
            if (cv.Liquid)
            {
                return 2;
            }
            Terrain t = GameState.World!.Terrains.GetWithBoundsChecked(x1, y1, z1);
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

        public void StepToValidEmptyTile(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            // this is where you'd fire some kind of event
            Entity.UnboxBriefly()!.PlaceInValidEmptyTile(x1, y1, z1);
            Actor a = Entity.UnboxBriefly()!.GetComponent<Actor>();
            a.Spend(16);
            //CachedActor.Spend(16);
        }

        // tentative...this does not allow (1) diagonal work/attacks or (2) digging upward...could handle the latter with ramps
        public bool CouldTouchBounded(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int dz = z1 - z0;
            int dx = x1 - x0;
            int dy = y1 - y0;
            if (dz == 0)
            {
                if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1)
                {
                    return true;
                }
            }
            else if (dz == +1 && dx == 0 && dy == 0 && GameState.World!.Terrains.GetWithBoundsChecked(x1, y1, z1).ZView == -1)
            {
                return true;
            }
            else if (dz == -1 && dx == 0 && dy == 0 && GameState.World!.Terrains.GetWithBoundsChecked(x0, y0, z0).ZView == -1)
            {
                return true;
            }
            return false;
        }

        public bool CanTouchBounded(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            var (x, y, z) = Entity.UnboxBriefly()!;
            return CouldTouchBounded((int)x!, (int)y!, (int)z!, x1, y1, z1);
        }



        //public Func<int, int, int, bool> DelegateCanMove()
        //{
        //    return (int x, int y, int z) => { return CanMove(x, y, z); };
        //}
        //public Func<int, int, int, bool> DelegateCanStand()
        //{
        //    return (int x, int y, int z) => { return CanStand(x, y, z); };
        //}

        public bool CanReachBounded(int x1, int y1, int z1, bool useLast = true)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            var e = Entity.UnboxBriefly()!;
            int x0 = (int)e.X!;
            int y0 = (int)e.Y!;
            int z0 = (int)e.Z!;
            Func<int, int, int, int, int, int, bool> movable;
            Func<int, int, int, bool> standable;
            movable = CouldMoveBounded;
            standable = CanStandBounded;
            var path = Tiles.FindPath(this, x1, y1, z1, useLast: useLast, movable: movable, standable: standable);
            Coord? c = (path.Count > 0) ? path.First!.Value : (Coord?)null;
            return (c == null) ? false : true;
        }


        public bool CanReachBounded(TileEntity t, bool useLast = true, bool useCache = true)
        {
            if (!t.Placed)
            {
                return false;
            }    
            Func<int, int, int, int, int, int, bool> movable;
            Func<int, int, int, bool> standable;
            movable = CouldMoveBounded;
            standable = CanStandBounded;
            var path = Tiles.FindPath(this, t, useLast: useLast, movable: movable, standable: standable, useCache: useCache);
            Coord? c = (path.Count > 0) ? path.First!.Value : (Coord?)null;
            return (c == null) ? false : true;
        }

        public bool CanReachResources(Dictionary<Resource, int> resources, bool respectClaims = true, bool ownedOnly = true, bool alwaysNeedsIngredients = false, bool useCache = true)
        {
            if (HecatombOptions.NoIngredients && !alwaysNeedsIngredients)
            {
                return true;
            }
            var needed = new Dictionary<Resource, int>(resources);
            List<Item> items = Items.Where(
                it => { return (needed.ContainsKey(it.Resource!) && (ownedOnly == false || !it.Disowned) && (!respectClaims || it.Unclaimed > 0) && CanReachBounded(it, useCache: useCache)); }
            ).ToList();
            foreach (Item item in items)
            {
                if (needed.ContainsKey(item.Resource!))
                {
                    int n = (respectClaims) ? item.Unclaimed : item.N;
                    needed[item.Resource!] -= n;
                    if (needed[item.Resource!] <= 0)
                    {
                        needed.Remove(item.Resource!);
                    }
                }
            }
            return (needed.Count == 0);
        }

        public bool CanReachResource(Resource resource, int need, bool respectClaims = true, bool ownedOnly = true, bool useCache = true)
        {
            if (HecatombOptions.NoIngredients)
            {
                return true;
            }
            List<Item> items = Items.Where(
                it => { return (it.Resource == resource && (ownedOnly == false || !it.Disowned) && (!respectClaims || it.Unclaimed > 0) && CanReachBounded(it, useCache: useCache)); }
            ).ToList();
            int needed = need;
            foreach (Item item in items)
            {
                int n = (respectClaims) ? item.Unclaimed : item.N;
                needed -= n;
            }
            return (needed <= 0);
        }
    }
}
