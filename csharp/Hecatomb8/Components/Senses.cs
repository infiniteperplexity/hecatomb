using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class Senses : Component
    {
        public int Range = 15;
        [JsonIgnore] public HashSet<Coord> Visible = new HashSet<Coord>();
        [JsonIgnore] private int storedZ;

        public HashSet<Coord> GetFOV()
        {
            resetVisible();
            var (x, y, z) = Entity.UpdateNullity().UnboxIfNotNull()!;
            storedZ = (int)z!;
            ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting((int)x!, (int)y!, Range, cannotSeeThrough, addToVisible);
            foreach (Coord c in Visible.ToList())
            {
                if (Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z + 1).ZView == -1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z + 1));
                }
                if (Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z).ZView == -1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z - 1));
                }
            }
            return Visible;
        }

        public bool CanSeeThrough(int x, int y)
        {
            return !cannotSeeThrough(x, y);
        }

        private bool cannotSeeThrough(int x, int y)
        {
            return GameState.World!.Terrains.GetWithBoundsChecked(x, y, storedZ).Opaque;
        }
        private void resetVisible()
        {
            Visible = new HashSet<Coord>();
        }

        private void addToVisible(int x, int y)
        {
            Visible.Add(new Coord(x, y, storedZ));
        }



        // some callback-heavy code to find nearby, visible things
        //private Func<int, int, int, bool> storedCallback;
        //private Coord storedCoord;
        //private Actor? storedActor;
        //private Movement? storedMovement;
        //public Coord FindClosestVisible(Func<int, int, int, bool> where = null)
        //{
        //    var (x, y, z) = Entity;
        //    storedZ = z;
        //    storedCallback = where;
        //    storedCoord = new Coord();
        //    ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, lookAround);
        //    return storedCoord;
        //}
        //private void lookAround(int x, int y)
        //{
        //    var c = storedCoord;
        //    var z = storedZ;
        //    var (X, Y, Z) = Entity;
        //    if (!c.Equals(default(Coord)) && Tiles.QuickDistance(c.X, c.Y, c.Z, X, Y, Z) <= Tiles.QuickDistance(x, y, z, X, Y, Z))
        //    {
        //        return;
        //    }
        //    if (storedCallback(x, y, z))
        //    {
        //        storedCoord = new Coord(x, y, z);
        //    }
        //    else if (Terrains[x, y, z + 1].ZView == -1 && storedCallback(x, y, z + 1))
        //    {
        //        storedCoord = new Coord(x, y, z + 1);
        //    }
        //    else if (Terrains[x, y, z].ZView == -1 && storedCallback(x, y, z - 1))
        //    {
        //        storedCoord = new Coord(x, y, z - 1);
        //    }
        //}
    }
}
