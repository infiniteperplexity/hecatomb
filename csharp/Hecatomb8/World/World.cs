using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    // This is the top level container object for all the game state data
    class World
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        //public int[,,] Lighting;
        //public int[,,] Outdoors;

        public Creature? Player;
        public Grid3D<Terrain> Terrains;
        public Grid3D<Cover> Covers;
        public readonly Dictionary<int, Entity> Entities;
        public readonly SparseArray3D<Creature> Creatures;
        public readonly StatefulRandom Random;

        public World(int width, int height, int depth, int seed = 0)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Entities = new Dictionary<int, Entity>();
            Terrains = new Grid3D<Terrain>(width, height, depth);
            Covers = new Grid3D<Cover>(width, height, depth);
            Creatures = new SparseArray3D<Creature>(width, height, depth);
            Random = new StatefulRandom(seed);
        }

        public int GetBoundedGroundLevel(int x, int y)
        {
            if (x <= 0 || x >= Width - 1 || y <= 0 || y >= Height - 1)
            {
                throw new IndexOutOfRangeException(String.Format("Cannot get GroundLevel for column {0} {1}.", x, y));
            }
            int elev = Depth - 1;
            for (int i = Depth - 1; i > 0; i--)
            {
                if (Terrains.GetWithBoundsChecked(x, y, i) == Terrain.FloorTile || Terrains.GetWithBoundsChecked(x, y, i) == Terrain.UpSlopeTile)
                {
                    return i;
                }
                //if (Terrains[x,y,i].Solid)
                //{
                //	return i+1;
                //}
            }
            return 1;
        }

    }
}
