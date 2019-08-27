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
    class GhoulTracker : StateHandler
    {
        public GhoulTracker()
        {
            AddListener<PlaceEvent>(OnStep);
            //AddListener();
        }

        public GameEvent OnStep(GameEvent ge)
        {
            if (Game.Options.NoGhouls)
            {
                return ge;
            }
            PlaceEvent pe = (PlaceEvent)ge;
            if (pe.Entity is Creature)
            {
                Creature cr = (Creature)pe.Entity;
                // Every step you take far from home, you might spawn a ghoul if a grave is nearby
                if (cr.TypeName == "Necromancer" || cr.TypeName=="Zombie")
                {
                    var (x, y, z) = cr;
                    // only do this if you're walking on the surface
                    if (z != Game.World.GetGroundLevel(x, y))
                    {
                        return ge;
                    }
                    int x0 = Game.World.Width / 2;
                    int y0 = Game.World.Height / 2;
                    int z0 = Game.World.GetGroundLevel(x0, y0);
                    // only if you're far from the center
                    if (Tiles.QuickDistance(x, y, z, x0, y0, z0)>=25)
                    {
                        foreach (Feature f in Game.World.Features.ToList())
                        {
                            if (f.TypeName=="Grave" && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z) <=8)
                            {
                                if (Game.World.Random.Next(500)==0 && Game.World.Tasks[f.X, f.Y, f.Z]==null)
                                {
                                    EmergeGhoul(f);
                                }
                            }
                        }
                    }
                }
            }
            return ge;
        }

        public void EmergeGhoul(Feature f)
        {
            var (x, y, z) = f;
            ParticleEmitter emitter = new ParticleEmitter();
            emitter.Place(x, y, z);
            f.Destroy();
            foreach (Coord c in Tiles.GetNeighbors8(x, y, z))
            {
                int x1 = c.X;
                int y1 = c.Y;
                int z1 = c.Z;
                f = Game.World.Features[x1, y1, z1];
                if (Game.World.Features[x1, y1, z1] == null && !Game.World.Terrains[x1, y1, z1].Solid && !Game.World.Terrains[x1, y1, z1].Fallable)
                {
                    if (Game.World.Random.Next(2) == 0)
                    {
                        Item.PlaceNewResource("Rock", 1, x1, y1, z1, owned: false);
                    }
                }
            }
            Game.World.Terrains[x, y, z] = Terrain.DownSlopeTile;
            Game.World.Terrains[x, y, z - 1] = Terrain.UpSlopeTile;
            Game.World.Covers[x, y, z] = Cover.NoCover;
            Game.World.Covers[x, y, z - 1] = Cover.NoCover;
            Creature ghoul = Entity.Spawn<Creature>("HungryGhoul");
            ghoul.Place(x, y, z - 1);
            if (Game.World.Random.Next(10) == 0)
            {
                ghoul.GetComponent<Inventory>().Item = Item.SpawnNewResource("TradeGoods", 1);
            }
            if (Game.Visible.Contains(new Coord(x, y, z)) || Game.Options.Visible)
            {
                Game.InfoPanel.PushMessage("{orange}A ravenous ghoul bursts forth from its grave!");
            }
        }
    }
}
