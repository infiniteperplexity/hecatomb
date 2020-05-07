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
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {

                return Visible;
            }
            var (x, y, z) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
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

        public static void Announce(int x, int y, int z, ColoredText? sight = null, ColoredText? sound = null, int soundRange = 12)
        {
            Coord c = new Coord(x, y, z);
            if (InterfaceState.PlayerVisible.Contains(c) && sight != null)
            {
                PushMessage(sight);
            }
            else if (sound != null)
            {
                if (Tiles.Distance(x, y, z, (int)Player.X!, (int)Player.Y!, (int)Player.Z!)<=soundRange)
                {
                    PushMessage(sound);
                    return;
                }
                foreach (var cr in GetState<TaskHandler>().GetMinions())
                {
                    if (Tiles.Distance(x, y, z, (int)cr.X!, (int)cr.Y!, (int)cr.Z!) <= soundRange)
                    {
                        if (Tiles.Distance(x, y, z, (int)Player.X!, (int)Player.Y!, (int)Player.Z!) <= soundRange)
                        {
                            PushMessage(sound);
                            return;
                        }
                    }
                }
            }
        }

        // some callback-heavy code to find nearby, visible things
        [JsonIgnore] private Func<int, int, int, bool>? storedCallback;
        [JsonIgnore] private Coord? storedCoord;
        [JsonIgnore] private Actor? storedActor;
        [JsonIgnore] private Movement? storedMovement;
        [JsonIgnore] private int storedX;
        [JsonIgnore] private int storedY;
        // storedX, storedY, and storedZ are the coordinates of the entity that's doing the finding
        // storedCoord is the best coordinate yet found
        public Coord? FindClosestVisible(Func<int, int, int, bool>? where = null)
        {
            if (Entity?.UnboxBriefly() is null)
            {
                return null;
            }
            var entity = Entity.UnboxBriefly()!;
            if (!entity.Placed)
            {
                return null;
            }
            (storedX, storedY, storedZ) = entity.GetPlacedCoordinate();
            storedCallback = where;
            storedCoord = null;
            ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(storedX, storedY, Range, cannotSeeThrough, lookAround);
            return storedCoord;
        }

        private void lookAround(int x, int y)
        {
            var z = storedZ;
            // if there's already a stored coordinate, and this coordinate is further away, bail
            if (storedCoord != null && Tiles.Distance(x, y, z, storedX, storedY, storedZ) >= Tiles.Distance(((Coord)storedCoord).X, ((Coord)storedCoord).Y, ((Coord)storedCoord).Z, storedX, storedY, storedZ))
            {
                return;
            }
            if (storedCallback is null || storedCallback(x, y, z))
            {
                storedCoord = new Coord(x, y, z);
            }
            else if (Terrains.GetWithBoundsChecked(x, y, z + 1).ZView == -1 && storedCallback(x, y, z + 1))
            {
                storedCoord = new Coord(x, y, z + 1);
            }
            else if (Terrains.GetWithBoundsChecked(x, y, z).ZView == -1 && storedCallback(x, y, z - 1))
            {
                storedCoord = new Coord(x, y, z - 1);
            }
        }
    }
}
