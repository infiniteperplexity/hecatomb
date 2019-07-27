/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class Senses : Component
    {
        public int Range = 10;
        [JsonIgnore] public HashSet<Coord> Visible;
        [JsonIgnore] private int storedZ;

        public HashSet<Coord> GetFOV()
        {
            resetVisible();
            var (x, y, z) = Entity;
            storedZ = z;
            ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, addToVisible);
            foreach (Coord c in Visible.ToList())
            {
                if (Terrains[c.X, c.Y, c.Z + 1].ZView == -1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z + 1));
                }
                if (Terrains[c.X, c.Y, c.Z].ZView == -1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z - 1));
                }
            }
            return Visible;
        }

        public void Explore()
        {
            Actor a = Entity.GetComponent<Actor>();
            if (a.Team == Teams.Friendly)
            {
                var visible = GetFOV();
                foreach (Coord c in visible)
                {
                    Explored.Add(c);
                }
            }
        }

        public bool CanSeeThrough(int x, int y)
        {
            return cannotSeeThrough(x, y);
        }

        private bool cannotSeeThrough(int x, int y)
        {
            return Game.World.GetTile(x, y, storedZ).Opaque;
        }
        private void resetVisible()
        {
            Visible = new HashSet<Coord>();
        }

        private void addToVisible(int x, int y)
        {
            Visible.Add(new Coord(x, y, storedZ));
        }

        private Creature storedCreature;
        private Actor storedActor;
        private Movement storedMovement;
        public Creature GetVisibleEnemy()
        {
            var (x, y, z) = Entity;
            storedZ = z;
            storedCreature = null;
            storedActor = Entity.GetComponent<Actor>();
            storedMovement = Entity.GetComponent<Movement>();
            ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, checkForEnemy);
            return storedCreature;
        }
        private void checkForEnemy(int x, int y)
        {
            for (int i = 1; i >= -1; i--)
            {
                Creature cr = Creatures[x, y, storedZ + i];
                if (cr != null)
                {
                    if (storedActor.IsHostile(cr))
                    {
                        if (storedMovement.CanReach(cr))
                        {
                            if (storedCreature == null || Tiles.QuickDistance(x, y, storedZ, Entity.X, Entity.Y, Entity.Z) < Tiles.QuickDistance(storedCreature.X, storedCreature.Y, storedCreature.Z, Entity.X, Entity.Y, Entity.Z))
                            {
                                storedCreature = cr;
                            }
                        }
                    }
                }
            }
        }
    }
}
