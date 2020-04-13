using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hecatomb
{
    using static HecatombAliases;
    public class Item : TileEntity
    {
        public bool Owned;
        public int Quantity;
        public int Claimed;
        public string Resource;
        public string CorpseSpecies;
        public int Decay;
        public int TotalDecay;
        [JsonIgnore]
        public int StackSize
        {
            get
            {
                return Hecatomb.Resource.Types[Resource].StackSize;
            }
            set
            { }
        }

        public int Unclaimed
        {
            get
            {
                return Quantity - Claimed;
            }
        }

        public Item() : base()
        {
            Quantity = 1;
            Claimed = 0;
            StackSize = 1;
            Owned = true;
        }

        public bool IsHauled()
        {
            foreach (var task in Tasks)
            {
                if (task is HaulTask && task.Claims.ContainsKey(EID))
                {
                    return true;
                }
            }
            return false;
        }

        public static Item SpawnCorpse()
        {
            return SpawnCorpse("Human");
        }


        public static Item SpawnCorpse(string creatureType)
        {
            // when you harvest a tombstone, it creatures a human corpse?
            Item item = Entity.Spawn<Item>();
            item.Resource = "Corpse";
            item.CorpseSpecies = creatureType;
            item.TotalDecay = 250;
            item.Decay = item.TotalDecay;
            OldGame.World.Events.Subscribe<TurnBeginEvent>(item, item.CorpseDecays);
            return item;
        }

        public string GetCalculatedFG()
        {
            if (TotalDecay > 0)
            {
                double frac = (double)Decay / (double)TotalDecay;
                if (frac < 0.25)
                {
                    return "purple";
                }
                else if (frac < 0.5)
                {
                    return "olive";
                }
            }
            return Hecatomb.Resource.GetResourceColor(Resource);
        }

        public GameEvent CorpseDecays(GameEvent ge)
        {
            Decay -= 1;
            if (Decay == 0)
            {
                Destroy();
            }
            return ge;
        }
        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Item e = OldGame.World.Items[x1, y1, z1];
            if (e == null)
            {
                OldGame.World.Items[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else if (e.Resource == Resource && e.Quantity < e.StackSize)
            {
                // should I fire a Place event here?
                e.AddQuantity(Quantity);
                Despawn();
            }
            else
            {
                Displace(x1, y1, z1);
            }
        }

        public override void Fall()
        {
            Place(X, Y, Z - 1);
            base.Fall();
        }
        public override void Remove()
        {
            int x0 = X;
            int y0 = Y;
            int z0 = Z;
            base.Remove();
            OldGame.World.Items[x0, y0, z0] = null;
        }

        public Item Take(int n)
        {
            Quantity -= n;
            Item item = Spawn<Item>();
            item.Resource = Resource;
            item.Quantity = n;
            item.Owned = Owned;
            if (Quantity <= 0)
            {
                Despawn();
            }
            return item;
        }
        public Item TakeClaimed(int n)
        {
            Claimed -= n;
            return Take(n);
        }

        public static Item SpawnNewResource(string r, int n)
        {
            Item item = (r == "Corpse") ? SpawnCorpse() : Spawn<Item>();
            item.Resource = r;
            item.Quantity = n;
            return item;
        }
        public static Item PlaceNewResource(string r, int n, int x, int y, int z, bool owned = true)
        {
            Item item = (r == "Corpse") ? SpawnCorpse() : Spawn<Item>();
            item.Resource = r;
            item.Quantity = n;
            item.Owned = owned;
            item.Place(x, y, z);
            return item;
        }

        public bool IsStored()
        {
            Feature f = Features[X, Y, Z];
            StructuralComponent sc = f?.TryComponent<StructuralComponent>();
            if (sc != null)
            {
                if (sc.Structure.Unbox().Stores.Contains(Resource))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddQuantity(int n)
        {
            int leftovers = 0;
            if (Quantity + n > StackSize)
            {
                leftovers = n - (StackSize - Quantity);
                Quantity = StackSize;
                PlaceNewResource(Resource, leftovers, X, Y, Z);
            }
            else
            {
                Quantity += n;
            }
        }

        public void Displace(int x, int y, int z)
        {
            int MaxDistance = 2;
            List<int> order = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            order = order.OrderBy(s => OldGame.World.Random.Arbitrary(OwnSeed()+s)).ToList();
            //order = order.OrderBy(s => Game.World.Random.NextDouble()).ToList();
            Queue<Coord> queue = new Queue<Coord>();
            if (Terrains[x, y, z] == Terrain.DownSlopeTile)
            {
                // first try to roll it down a slope
                queue.Enqueue(new Coord(x, y, z - 1));
            }
            foreach (int i in order)
            {
                // add all eight neighbors to the queue
                var (dx, dy, dz) = Movement.Directions8[i];
                queue.Enqueue(new Coord(x + dx, y + dy, z + dz));
            }
            HashSet<Coord> tried = new HashSet<Coord>();
            while (queue.Count > 0)
            {
                Coord c = queue.Dequeue();
                tried.Add(c);
                if (Terrains[c.X, c.Y, c.Z].Solid)
                {
                    continue;
                }
                if (Items[c] != null)
                {
                    if (Items[c].Resource != Resource || Items[c].Quantity >= StackSize)
                    {
                        if (Terrains[c.X, c.Y, c.Z] == Terrain.DownSlopeTile)
                        {
                            Coord d = new Coord(c.X, c.Y, c.Z - 1);
                            if (!tried.Contains(d))
                            {
                                queue.Enqueue(d);
                            }
                        }
                        foreach (int i in order)
                        {
                            var (dx, dy, dz) = Movement.Directions8[i];
                            dx += c.X;
                            dy += c.Y;
                            dz += c.Z;
                            Coord d = new Coord(dx, dy, dz);
                            if (!tried.Contains(d) && Tiles.QuickDistance(x, y, z, dx, dy, dz) <= MaxDistance)
                            {
                                queue.Enqueue(d);
                            }

                        }
                        continue;
                    }
                }
                OldGame.World.Events.Publish(new SensoryEvent($"{Describe(capitalized: true)} got displaced to {c.X} {c.Y} {c.Z}.",c.X, c.Y, c.Z));
                Place(c.X, c.Y, c.Z);
                return;
            }
            Status.PushMessage($"{Describe()} became lost in the clutter.");
            Despawn();
        }

        [JsonIgnore] Creature cachedMock;
        public override string Describe(
            bool article = true,
            bool definite = false,
            bool capitalized = false
        )
        {
            string name = Hecatomb.Resource.Types[Resource].Name;
            string str = "";
            
            if (Resource == "Corpse")
            {
                if (CorpseSpecies == "Human")
                {
                    str = "corpse";
                }
                else
                {
                    cachedMock = (cachedMock == null) ? Entity.Mock<Creature>(CorpseSpecies) : cachedMock;
                    str = cachedMock.Name + " corpse";
                }

                if (TotalDecay > 0)
                {
                    double frac = (double)Decay / (double)TotalDecay;
                    if (frac < 0.25)
                    {
                        str = "severely rotted " + str;
                    }
                    else if (frac < 0.5)
                    {
                        str = "rotted " + str;
                    }
                }
                if (article)
                {
                    if (definite)
                    {
                        str = "the " + str;
                    }
                    else
                    {
                        str = "a " + str;
                    }
                }
            }
            else
            {
                //return $"{Quantity} {name}";
                var unowned = (Owned) ? "" : " (unclaimed)";
                // return $"{Quantity} {name} ({Claimed} claimed, {txt}owned)";
                str = $"{Quantity} {name}{unowned}";
            }
            if (capitalized)
            {
                str = char.ToUpper(str[0]) + str.Substring(1);
            }
            return str;
        }


        public static Dictionary<string, int> CombinedResources(List<Dictionary<string, int>> list)
        {
            var total = new Dictionary<string, int>();
            foreach (var resources in list)
            {
                if (resources != null)
                {
                    foreach (string resource in resources.Keys)
                    {
                        if (!total.ContainsKey(resource))
                        {
                            total[resource] = 0;
                        }
                        total[resource] += resources[resource];
                    }
                }
            }
            return total;
        }
    }
}
