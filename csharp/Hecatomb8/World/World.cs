using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class World
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        public readonly Dictionary<int, Entity> Entities;
        public readonly SparseArray3D<Terrain> Terrains;
        public readonly SparseArray3D<Creature> Creatures;

        public World(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Entities = new Dictionary<int, Entity>();
            Terrains = new SparseArray3D<Terrain>(width, height, depth);
            Creatures = new SparseArray3D<Creature>(width, height, depth);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        Terrain t;
                        if (i == 0 || j == 0 || k == 0 || i == width - 1 || j == height - 1 || k == depth - 1)
                        {
                            t = Terrain.VoidTile;
                        }
                        else
                        {
                          
                        }
                    }
                }
            }
        }
    }
}
