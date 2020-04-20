using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public class Movement : Component
    {
        public bool Walks;
        public bool Flies;
        public bool Swims;
        public bool Phases;

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
            var e = Entity.UnboxBriefly();
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

        //public float MovementCost(int x0, int y0, int z0, int x1, int y1, int z1)
        //{
        //    //Feature f = Game.World.Features[x1, y1, z1];
        //    //if (f != null && f.Solid && CachedActor.Team != Teams.Friendly)
        //    //{
        //    //    return 12;
        //    //}
        //    Cover cv = Game.World.Covers[x1, y1, z1];
        //    if (cv.Liquid)
        //    {
        //        return 2;
        //    }
        //    Terrain t = Game.World.Terrains[x1, y1, z1];
        //    if (t == Terrain.UpSlopeTile || t == Terrain.DownSlopeTile)
        //    {
        //        return 2;
        //    }
        //    if (z1 > z0)
        //    {
        //        return 3;
        //    }
        //    return 1;
        //}

        public void StepToValidEmptyTile(int x1, int y1, int z1)
        {
            Entity.UnboxBriefly()!.PlaceInValidEmptyTile(x1, y1, z1);
            //Actor a = Entity.GetComponent<Actor>();
            //a.Spend(16);
            //CachedActor.Spend(16);
        }

        // tentative...this does not allow (1) diagonal work/attacks or (2) digging upward...could handle the latter with ramps
        //public bool CouldTouch(int x0, int y0, int z0, int x1, int y1, int z1)
        //{
        //    int dz = z1 - z0;
        //    int dx = x1 - x0;
        //    int dy = y1 - y0;
        //    if (dz == 0)
        //    {
        //        if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1)
        //        {
        //            return true;
        //        }
        //    }
        //    else if (dz == +1 && dx == 0 && dy == 0 && Game.World.Terrains[x1, y1, z1].ZView == -1)
        //    {
        //        return true;
        //    }
        //    else if (dz == -1 && dx == 0 && dy == 0 && Game.World.Terrains[x0, y0, z0].ZView == -1)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //public bool CanTouch(int x1, int y1, int z1)
        //{
        //    return CouldTouch(Entity.X, Entity.Y, Entity.Z, x1, y1, z1);
        //}



        //public Func<int, int, int, bool> DelegateCanMove()
        //{
        //    return (int x, int y, int z) => { return CanMove(x, y, z); };
        //}
        //public Func<int, int, int, bool> DelegateCanStand()
        //{
        //    return (int x, int y, int z) => { return CanStand(x, y, z); };
        //}

        //public bool CanReach(int x1, int y1, int z1, bool useLast = true)
        //{
        //    int x0 = Entity.X;
        //    int y0 = Entity.Y;
        //    int z0 = Entity.Z;
        //    Func<int, int, int, int, int, int, bool> movable;
        //    Func<int, int, int, bool> standable;
        //    movable = CouldMove;
        //    standable = CanStand;
        //    var path = Tiles.FindPath(this, x1, y1, z1, useLast: useLast, movable: movable, standable: standable);
        //    Coord? c = (path.Count > 0) ? path.First.Value : (Coord?)null;
        //    return (c == null) ? false : true;
        //}


        //public bool CanReach(TileEntity t, bool useLast = true, bool useCache = true)
        //{
        //    Func<int, int, int, int, int, int, bool> movable;
        //    Func<int, int, int, bool> standable;
        //    movable = CouldMove;
        //    standable = CanStand;
        //    var path = Tiles.FindPath(this, t, useLast: useLast, movable: movable, standable: standable, useCache: useCache);
        //    Coord? c = (path.Count > 0) ? path.First.Value : (Coord?)null;
        //    return (c == null) ? false : true;
        //}

        //public bool CanFindResources(Dictionary<string, int> resources, bool respectClaims = true, bool ownedOnly = true, bool alwaysNeedsIngredients = false, bool useCache = true)
        //{
        //    if (Game.Options.NoIngredients && !alwaysNeedsIngredients)
        //    {
        //        return true;
        //    }
        //    Dictionary<string, int> needed = new Dictionary<string, int>(resources);
        //    List<Item> items = Game.World.Items.Where(
        //        it => { return (needed.ContainsKey(it.Resource) && (ownedOnly == false || it.Owned) && (!respectClaims || it.Unclaimed > 0) && CanReach(it, useCache: useCache)); }
        //    ).ToList();
        //    foreach (Item item in items)
        //    {
        //        if (needed.ContainsKey(item.Resource))
        //        {
        //            int n = (respectClaims) ? item.Unclaimed : item.Quantity;
        //            needed[item.Resource] -= n;
        //            if (needed[item.Resource] <= 0)
        //            {
        //                needed.Remove(item.Resource);
        //            }
        //        }
        //    }
        //    return (needed.Count == 0);
        //}

        //public bool CanFindResource(string resource, int need, bool respectClaims = true, bool ownedOnly = true, bool useCache = true)
        //{
        //    if (Game.Options.NoIngredients)
        //    {
        //        return true;
        //    }
        //    List<Item> items = Game.World.Items.Where(
        //        it => { return (it.Resource == resource && (ownedOnly == false || it.Owned) && (!respectClaims || it.Unclaimed > 0) && CanReach(it, useCache: useCache)); }
        //    ).ToList();
        //    int needed = need;
        //    foreach (Item item in items)
        //    {
        //        int n = (respectClaims) ? item.Unclaimed : item.Quantity;
        //        needed -= n;
        //    }
        //    return (needed <= 0);
        //}
    }
}
