using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    class CaveVaultTracker : StateHandler
    {
        // the central coordinate, plus all the triggering coordinates nearby
        public Dictionary<Coord, Coord> TriggerZones;
        public HashSet<Coord> Triggered;

        public CaveVaultTracker()
        {
            AddListener<DigEvent>(OnDig);
            TriggerZones = new Dictionary<Coord, Coord>();
            Triggered = new HashSet<Coord>();
        }

        public GameEvent OnDig(GameEvent ge)
        {
            DigEvent de = (DigEvent)ge;
            // this is not the actual conditional triggering logic
            Coord c = new Coord(de.X, de.Y, de.Z);
            if (TriggerZones.ContainsKey(c))
            {
                Game.StatusPanel.PushMessage("{orange}A swarm of carnivorous bats bursts from the cave.");
                TriggerZone(c);
            }
            return ge;
        }

        public void TriggerZone(Coord c)
        {
            // never trigger a blob of zones more than once
            if (Triggered.Contains(c))
            {
                return;
            }
            Triggered.Add(c);
            Coord c0 = TriggerZones[c];
            var (x0, y0, z) = c0;          
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    int x1 = x0 + dx;
                    int y1 = y0 + dy;
                    Coord c1 = new Coord(x1, y1, z);
                    if (TriggerZones.ContainsKey(c1))
                    {
                        // if this zone overlaps with a not-yet-triggered zone, trigger that one
                        if (!Triggered.Contains(TriggerZones[c1]))
                        {
                            TriggerZone(c1);
                        }
                        TriggerZones.Remove(new Coord(x1, y1, z));
                    }
                    if (Tiles.QuickDistance(x1, y1, z, x0, y0, z) <= 1)
                    {
                        if (Game.World.Terrains[x1, y1, z].Solid)
                        {
                            Game.World.Terrains[x1, y1, z] = Terrain.FloorTile;
                            Game.World.Covers[x1, y1, z] = Cover.NoCover;
                            if (Game.World.Creatures[x1, y1, z] == null)
                            {
                                var bat = Entity.Spawn<Creature>("VampireBat");
                                bat.Place(x1, y1, z);
                            }
                        }
                    }
                }
            }
        }

        public void PlaceBatCaves()
        {
            PlaceBatCaves(64);
        }

        public void PlaceBatCaves(int n)
        {
            int ncaves = n;
            int border = 2;
            for (int z = 1; z <= 46; z++)
            {
                for (int i = 0; i < ncaves; i++)
                {
                    int x0 = Game.World.Random.Next(1 + border, Game.World.Width - 2 - border);
                    int y0 = Game.World.Random.Next(1 + border, Game.World.Height - 2 - border);
                    Coord c = new Coord(x0, y0, z);
                    // a marker for now
                    Game.World.Covers[x0, y0, z] = Cover.Bedrock;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            int x1 = x0 + dx;
                            int y1 = y0 + dy;
                            if (Tiles.QuickDistance(x1, y1, z, x0, y0, z) <= 2)
                            {
                                if (!TriggerZones.ContainsKey(new Coord(x1, y1, z)))
                                {
                                    TriggerZones[new Coord(x1, y1, z)] = c;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
