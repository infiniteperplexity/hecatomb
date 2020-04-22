using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Hecatomb8
{
    // This is the top level container object for all the game state data
    public partial class World
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        //public int[,,] Lighting;
        //public int[,,] Outdoors;

        public Creature? Player;
        public HashSet<Coord> Explored;
        public Grid3D<Terrain> Terrains;
        public Grid3D<Cover> Covers;
        public readonly Dictionary<int, Entity> Entities;
        public readonly SparseArray3D<Creature> Creatures;
        public readonly SparseArray3D<Feature> Features;
        public readonly SparseArray3D<Item> Items;
        public readonly SparseArray3D<Task> Tasks;
        public Dictionary<string, StateHandler> StateHandlers;
        public readonly StatefulRandom Random;

        public World(int width, int height, int depth, int seed = 0)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Entities = new Dictionary<int, Entity>();
            Explored = new HashSet<Coord>();
            Terrains = new Grid3D<Terrain>(width, height, depth);
            Covers = new Grid3D<Cover>(width, height, depth);
            Creatures = new SparseArray3D<Creature>(width, height, depth);
            Features = new SparseArray3D<Feature>(width, height, depth);
            Items = new SparseArray3D<Item>(width, height, depth);
            Tasks = new SparseArray3D<Task>(width, height, depth);
            StateHandlers = new Dictionary<string, StateHandler>();
            Random = new StatefulRandom(seed);
        }

        public T GetState<T>() where T : StateHandler, new()
        {
            string s = typeof(T).Name;
            if (!StateHandlers.ContainsKey(s))
            {
                var handler = Entity.Spawn<T>();
                StateHandlers[s] = handler;

            }
            return (T)StateHandlers[s];
        }

        public void SpawnHandlers()
        {
            List<Type> handlers = typeof(StateHandler).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StateHandler))).ToList();
            foreach (var type in handlers)
            {
                var handler = Entity.Spawn<StateHandler>(type);
                StateHandlers[type.Name] = handler;
            }
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
            }
            return 1;
        }

    }
}
