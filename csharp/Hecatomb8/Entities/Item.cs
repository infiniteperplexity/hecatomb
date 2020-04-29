using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Item : TileEntity
    {
        public Resource? Resource;
        public int N;
        public int Claimed;
        public int Unclaimed { get => N - Claimed;}
        public bool Disowned;
        public int StackSize;

        protected override string? getName()
        {
            return Resource!.Name;
        }
        protected override string? getFG()
        {
            return Resource!.FG;
        }
        protected override char getSymbol()
        {
            return Resource!.Symbol;
        }

        public Item()
        {
            StackSize = 5;
            N = 1;
        }
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Items.GetWithBoundsChecked(x, y, z) != null)
            {
                throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Items.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
            }
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Items.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
            GameState.World!.Items.SetWithBoundsChecked(x, y, z, this);
            base.PlaceInValidEmptyTile(x, y, z);
        }

        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Items.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
        }

        public void DropOnValidTile(int x, int y, int z)
        {
            var existing = GameState.World!.Items.GetWithBoundsChecked(x, y, z);
            if (existing is null)
            {
                PlaceInValidEmptyTile(x, y, z);
            }
            else if (existing.Resource == Resource)
            {
                int space = StackSize - existing.N;
                if (space <= 0)
                {
                    Tumble(x, y, z);
                }
                else if (space <= N)
                {
                    existing.N += N;
                    Despawn();
                }
                else
                {
                    existing.N += space;
                    N -= space;
                    Tumble(x, y, z);
                }
            }
            else
            {
                Tumble(x, y, z);
            }
        }

        public void Tumble(int x, int y, int z)
        {
            Item? existing = Items.GetWithBoundsChecked(x, y, z);
            if (existing is null)
            {
                // this should rarely happen
                PlaceInValidEmptyTile(x, y, z);
            }
            // the first thing that should happen is that if the tile above is an empty upslopetile, we place it there
            else if (Terrains.GetWithBoundsChecked(x, y, z + 1) == Terrain.DownSlopeTile && Items.GetWithBoundsChecked(x, y, z + 1) is null)
            {
                PlaceInValidEmptyTile(x, y, z + 1);
            }
            else
            {
                List<int> order = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                order = order.OrderBy(s => GameState.World!.Random.NextDouble()).ToList();
                Queue<Coord> queue = new Queue<Coord>();
                // first try to roll it down a slope
                if (Terrains.GetWithBoundsChecked(x, y, z) == Terrain.DownSlopeTile)
                {
                    queue.Enqueue(new Coord(x, y, z - 1));
                }
                foreach (int i in order)
                {
                    // add all eight neighbors to the queue
                    var (dx, dy, dz) = Coord.Directions8[i];
                    queue.Enqueue(new Coord(x + dx, y + dy, z + dz));
                }
                while (queue.Count > 0 && N > 0)
                {
                    Coord c = queue.Dequeue();
                    if (Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z).Solid)
                    {
                        continue;
                    }
                    existing = Items.GetWithBoundsChecked(c.X, c.Y, c.Z);
                    if (existing is null)
                    {
                        PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                        return;
                    }
                    else
                    {
                        if (existing.Resource == Resource)
                        {
                            int space = StackSize - existing.N;
                            if (space <= 0)
                            {
                                continue;
                            }
                            else if (space <= N)
                            {
                                existing.N += N;
                            }
                            else
                            {
                                existing.N += space;
                                N -= space;
                                continue;
                            }
                        }
                    }
                }
                if (N > 0)
                {
                    Senses.Announce(x, y, z, sight: $"{Describe(capitalized: true)} got lost in the clutter.");
                }
                Despawn();
            }
        }

        public static Item SpawnNewResource(Resource r, int n)
        {
            var item = Entity.Spawn<Item>();
            item.Resource = r;
            item.N = n;
            return item;
        }

        public Item Take(int n)
        {
            N -= n;
            Item item = Spawn<Item>();
            item.Resource = Resource;
            item.N = n;
            item.Disowned = Disowned;
            if (N <= 0)
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
    }
}
