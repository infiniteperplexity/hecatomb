﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    // This is the top level container object for all the game state data
    public partial class World
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        public int[,,] Lighting;
        public int[,,] Outdoors;

        public Creature? Player;
        public HashSet<Coord> Explored;
        public Grid3D<Terrain> Terrains;
        public Grid3D<Cover> Covers;
        public readonly Dictionary<int, Entity> Entities;
        public readonly SparseArray3D<Creature> Creatures;
        public readonly SparseArray3D<Feature> Features;
        public readonly SparseArray3D<Item> Items;
        public readonly SparseArray3D<Task> Tasks;
        // not super necessary but probably helps performance
        public HashSet<Structure> Structures;
        public Dictionary<string, int> StateHandlers;
        public StatefulRandom Random;
        public EventSystem Events;

        public World(int width, int height, int depth, int seed = 0)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Entities = new Dictionary<int, Entity>();
            Explored = new HashSet<Coord>();
            Lighting = new int[Width, Height, Depth];
            Outdoors = new int[Width, Height, Depth];
            Terrains = new Grid3D<Terrain>(width, height, depth);
            Covers = new Grid3D<Cover>(width, height, depth);
            Creatures = new SparseArray3D<Creature>(width, height, depth);
            Features = new SparseArray3D<Feature>(width, height, depth);
            Items = new SparseArray3D<Item>(width, height, depth);
            Tasks = new SparseArray3D<Task>(width, height, depth);
            StateHandlers = new Dictionary<string, int>();
            Structures = new HashSet<Structure>();
            
            Events = new EventSystem();

            Random = new StatefulRandom(seed);
            InterfaceState.Particles = new ListArray3D<Particle>(width, height, depth);
        }

        public T GetState<T>() where T : StateHandler, new()
        {
            string s = typeof(T).Name;
            if (!StateHandlers.ContainsKey(s))
            {
                var handler = Entity.Spawn<T>();
                StateHandlers[s] = (int)handler.EID!;

            }
            return (T)Entity.GetEntity<T>(StateHandlers[s])!;
        }

        public void SpawnHandlers()
        {
            List<Type> handlers = typeof(StateHandler).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StateHandler))).ToList();
            foreach (var type in handlers)
            {
                var handler = Entity.Spawn<StateHandler>(type);
                StateHandlers[type.Name] = (int)handler.EID!;
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
                if (Terrains.GetWithBoundsChecked(x, y, i).Floor)
                {
                    return i;
                }
            }
            return 1;
        }


        public void ValidateOutdoors()
        {
            GetState<PathHandler>().ResetPaths();
            Outdoors = new int[Width, Height, Depth];
            // by default it's full of 0s (indoors)
            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {

                    for (int z = Depth - 1; z > 0; z--)
                    {

                        // everything above ground is outdoors (2)
                        Outdoors[x, y, z] = 2;
                        foreach (Coord dir in Coord.Directions8)
                        {
                            var (dx, dy, _) = dir;
                            // this gets really weird because it tags walls as shaded.  This may change in the future.
                            if (Outdoors[x + dx, y + dy, z] == 0)
                            {
                                // if this square is outdoors, tag adjacent indoor squares as shaded (1)                      
                                Outdoors[x + dx, y + dy, z] = 1;
                            }
                        }
                        if (Terrains.GetWithBoundsChecked(x, y, z).Floor)
                        {
                            //Debug.WriteLine("breaking off at " + z);
                            //Debug.WriteLine(Outdoors[x, y, z]);
                            break;
                        }
                    }
                }
            }
        }

        public int GetLighting(int x, int y, int z)
        {
            int lighting = GetState<TurnHandler>().LightLevel;
            int outdoors = Outdoors[x, y, z];
            //outdoors = 2;
            if (outdoors == 0)
            {
                lighting = 0;
            }
            else if (outdoors == 1)
            {
                lighting = lighting / 2;
            }
            return lighting;
        }

    }
}
