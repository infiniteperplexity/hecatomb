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
    // is this going to handle all setpieces?
    public class Vault
    {
        public HashSet<Coord> Triggers;
        public HashSet<Coord> VaultTiles;
        public bool Awakened;
        public bool Populated;
        public int MessageId;
        public int Z
        {
            get
            {
                if (VaultTiles.Count==0)
                {
                    return -1;
                }
                return VaultTiles.ToList()[0].Z;
            }
            set
            {

            }

        }
        [JsonIgnore]
        public static List<ColoredText> Messages;

        static Vault()
        {
            Messages = new List<ColoredText>
            {
                "You breach a cavern.",
                "You breach a vault full of monsters!"
            };
        }
        public Vault()
        {
            Triggers = new HashSet<Coord>();
            VaultTiles = new HashSet<Coord>();
            MessageId = -1;
        }
       
        public void Listen(Coord c)
        {
            if (Triggers.Contains(c) || VaultTiles.Contains(c))
            {
                Awaken();
            }
        }
        public void Awaken()
        {
            if (Awakened)
            {
                return;
            }
            Awakened = true;
            foreach (Coord c in VaultTiles)
            {
                Creature cr = Game.World.Creatures[c];
                if (cr!=null)
                {
                    cr.GetComponent<Actor>().Asleep = false;
                }
            }
            Game.SplashPanel.Splash(new List<ColoredText> {
               Messages[MessageId]
            },
            logText: "{red}" + Messages[MessageId]);
        }

        public void Populate()
        {
            Populated = true;
            foreach (Coord tile in VaultTiles)
            {
                var (x, y, z) = tile;
                string creature;
                if (z >= 43)
                    creature = "VampireBat";
                else if (z >= 37)
                    creature = "FlourescentJelly";
                else if (z >= 31)
                    creature = "TroglodyteWarrior";
                else if (z >= 25)
                    creature = "BasaltElemental";
                else if (z >= 19)
                    creature = "NoctilucantOoze";
                else if (z >= 13)
                    creature = "TrollChampion";
                else
                    creature = "BasaltElemental";

                var bat = Entity.Spawn<Creature>(creature);
                bat.GetComponent<Actor>().Asleep = true;
                bat.Place(x, y, z);
            }
        }

        public void AcquireTriggers()
        {
            foreach (Coord c in VaultTiles)
            {
                var neighbors = Tiles.GetNeighbors8(c.X, c.Y, c.Z);
                foreach (Coord n in neighbors)
                {
                    if (!VaultTiles.Contains(n))
                    {
                        Triggers.Add(n);
                    }
                }
            }
        }
    }


    public class VaultHandler : StateHandler
    {
        public int Deepest;
        public List<Vault> Vaults;

        public VaultHandler()
        {
            Vaults = new List<Vault>();
            AddListener<DigEvent>(OnDig);
        }

        public GameEvent OnDig(GameEvent ge)
        {
            DigEvent de = (DigEvent)ge;
            // this is not the actual conditional triggering logic
            Coord c = new Coord(de.X, de.Y, de.Z);
            if (de.Z < Deepest)
            {
                Deepest = de.Z;
                foreach (Vault v in Vaults)
                {
                    if (!v.Populated && v.Z >= Deepest - 3)
                    {
                        v.Populate();
                    }
                }
            }
            foreach (Vault v in Vaults)
            {
                v.Listen(c);
            }
            if (de.EventType == "Pit" || de.EventType == "LevelPit" || de.EventType == "Hole")
            {
                Coord cb = new Coord(de.X, de.Y, de.Z - 1);
                foreach (Vault v in Vaults)
                {
                    v.Listen(cb);
                }
            }
            else if (de.EventType=="Ramp")
            {
                Coord ca = new Coord(de.X, de.Y, de.Z + 1);
                foreach (Vault v in Vaults)
                {
                    v.Listen(ca);
                }
            }
            return ge;
        }

        public void DigCaverns(int z)
        {
            var cells = new CellularAutomaton(Game.World.Width, Game.World.Height);
            cells.Initialize(p: 0.4);
            cells.DoSteps(birth: 45678, steps: 2);
            for (int x = 1; x < Game.World.Width - 1; x++)
            {
                for (int y = 1; y < Game.World.Height - 1; y++)
                {
                    if (!cells.Cells[x, y, 0])
                    {
                        Game.World.Terrains[x, y, z] = Terrain.FloorTile;
                        Game.World.Covers[x, y, z] = Cover.NoCover;
                    }
                }
            }
            int zone = 0;
            Dictionary<Coord, int> zones = new Dictionary<Coord, int>();

            // define a recursive flood-fill function
            Action<int, int> flood = null;
            flood = (int x, int y) =>
            {
                zones[new Coord(x, y, z)] = zone;
                var neighbors = Tiles.GetNeighbors8(x, y, z);
                foreach (var n in neighbors)
                {
                    if (!Game.World.Terrains[n.X, n.Y, n.Z].Solid && !zones.ContainsKey(n))
                    {
                        flood(n.X, n.Y);
                    }
                }
            };

            // fill out the zones...there's a problem in that you can't breach two zones at once...
            for (int x = 1; x < Game.World.Width - 1; x++)
            {
                for (int y = 1; y < Game.World.Height - 1; y++)
                {
                    var c = new Coord(x, y, z);
                    if (!Game.World.Terrains[x, y, z].Solid && !zones.ContainsKey(c))
                    {
                        flood(x, y);
                        zone += 1;
                    }
                }
            }
            Debug.WriteLine($"Cavern level {z} contains {zone} distinct zones");
            List<Vault> vaults = new List<Vault>();
            // create one vault for every zone
            for (int i=0; i<zone; i++)
            {
                Vault v = new Vault();
                vaults.Add(v);
            }
            // assign all coordinates to the appropriate vaults
            foreach(Coord c in zones.Keys)
            {
                int i = zones[c];
                vaults[i].VaultTiles.Add(c);
            }
            // grab edge tiles for each vault
            foreach (Vault v in vaults)
            {
                v.AcquireTriggers();
                v.MessageId = 0;
                Vaults.Add(v);
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
                    //Game.World.Covers[x0, y0, z] = Cover.Bedrock;
                    // if these overlap we should merge them
                    Vault v = new Vault();
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            int x1 = x0 + dx;
                            int y1 = y0 + dy;
                            if (Tiles.QuickDistance(x1, y1, z, x0, y0, z) <= 2 && Game.World.Terrains[x1, y1, z].Solid)
                            {
                                Game.World.Terrains[x1, y1, z] = Terrain.FloorTile;
                                Game.World.Covers[x1, y1, z] = Cover.NoCover;
                                v.VaultTiles.Add(new Coord(x1, y1, z));
                            }
                        }
                    }
                    v.AcquireTriggers();
                    v.MessageId = 1;
                    if (v.VaultTiles.Count > 0)
                    {
                        Vaults.Add(v);
                    }
                }
            }
            Deepest = 46;
            foreach (Vault v in Vaults)
            {
                // what's going on with this check?  Oh, that's right, we don't populate the rest until we dig down
                if (v.Z >= Deepest - 3)
                {
                    v.Populate();
                }
            }
        }
    }
}
