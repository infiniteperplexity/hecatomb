using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hecatomb
{
    using static HecatombAliases;
    public class Item : TileEntity
    {
        public bool Owned;
        public int Quantity;
        public int Claimed;
        public string Resource;
        public int StackSize;

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
        }

        public static Item SpawnCorpse()
        {
            Item item = Entity.Spawn<Item>();
            item.Resource = "Corpse";
            return item;
        }
        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Item e = Game.World.Items[x1, y1, z1];
            if (e == null)
            {
                Game.World.Items[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else if (e.Resource == Resource && e.Quantity < e.StackSize)
            {
                // should I fire a Place event here?
                Debug.WriteLine("adding quantity " + e.Quantity + " " + Quantity);
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
            Game.World.Items[x0, y0, z0] = null;
        }

        public Item Take(int n)
        {
            Quantity -= n;
            Item item = Spawn<Item>();
            item.Resource = Resource;
            item.Quantity = n;
            if (Quantity<=0)
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

        public static Item PlaceNewResource(string r, int n, int x, int y, int z, bool owned = true)
        {
            Item item = (r=="Corpse") ? SpawnCorpse() : Spawn<Item>();
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
            order = order.OrderBy(s=>Game.World.Random.NextDouble()).ToList();
            Queue<Coord> queue = new Queue<Coord>();
            if (Terrains[x, y, z] == Terrain.DownSlopeTile)
            {
                // first try to roll it down a slope
                queue.Enqueue(new Coord(x, y, z - 1));
            }
            foreach (int i in order)
            {
                var (dx, dy, dz) = Movement.Directions8[i];
                queue.Enqueue(new Coord(x + dx, y + dy, z + dz));
            }
            HashSet<Coord> tried = new HashSet<Coord>();
            while (queue.Count>0)
            {
                Coord c = queue.Dequeue();
                tried.Add(c);
                if (Terrains[c.X, c.Y, c.Z].Solid)
                {
                    continue;
                }
                if (Items[c]!=null)
                {
                    if (Items[c].Resource!=Resource || Items[c].Quantity>=StackSize)
                    {
                        if (Terrains[c.X, c.Y, c.Z] == Terrain.DownSlopeTile)
                        {
                            queue.Enqueue(new Coord(c.X, c.Y, c.Z-1));
                        }
                        foreach (int i in order)
                        {
                            var (dx, dy, dz) = Movement.Directions8[i];
                            queue.Enqueue(new Coord(x + dx, y + dy, z + dz));
                        }
                        foreach (int i in order)
                        {
                            var (dx, dy, dz) = Movement.Directions8[i];
                            dx += c.X;
                            dy += c.Y;
                            dz += c.Z;
                            Coord d = new Coord(dx, dy, dz);
                            if (!tried.Contains(d) && Tiles.QuickDistance(x, y, z, dx, dy, dz)<=MaxDistance)
                            {
                                queue.Enqueue(d);
                            }
                            
                        }
                        continue;
                    }
                }
                Status.PushMessage($"{Describe()} got displaced to {c.X} {c.Y} {c.Z}.");
                Place(c.X, c.Y, c.Z);
                return;
            }
            Status.PushMessage($"{Describe()} became lost in the clutter.");
            Despawn();
        }

        public override string Describe(
            bool article = true,
            bool definite = false,
            bool capitalized = false
        )
        {
            string name = Hecatomb.Resource.Types[Resource].Name;
            return $"{Quantity} {name}";
        }
    }
}
