using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb8
{
    public static class HecatombAliases
    {
        // GUI elements
        public static MainPanel MainPanel
        {
            get
            {
                return InterfaceState.MainPanel;
            }
        }

        public static ControlContext Controls
        {
            get
            {
                return InterfaceState.Controls;
            }
        }
        public static HecatombCommands Commands
        {
            get
            {
                return InterfaceState.Commands!;
            }
        }
        public static Colors Colors
        {
            get
            {
                return InterfaceState.Colors!;
            }
        }

        public static World World
        {
            get
            {
                return GameState.World!;
            }
        }
        public static Creature Player
        {
            get
            {
                return GameState.World!.Player!;
            }
        }
        public static HashSet<Coord> Explored
        {
            get
            {
                return GameState.World!.Explored;
            }
        }
        public static Grid3D<Terrain> Terrains
        {
            get
            {
                return GameState.World!.Terrains;
            }
        }
        public static Grid3D<Cover> Covers
        {
            get
            {
                return GameState.World!.Covers;
            }
        }
        public static SparseArray3D<Creature> Creatures
        {
            get
            {
                return GameState.World!.Creatures;
            }
        }
        public static SparseArray3D<Feature> Features
        {
            get
            {
                return GameState.World!.Features;
            }
        }
        public static SparseArray3D<Task> Tasks
        {
            get
            {
                return GameState.World!.Tasks;
            }
        }
        public static SparseArray3D<Item> Items
        {
            get
            {
                return GameState.World!.Items;
            }
        }

        public static Dictionary<int, Entity> Entities
        {
            get
            {
                return GameState.World!.Entities;
            }
        }

        public static void Publish(GameEvent ge)
        {
            GameState.World!.Events.Publish(ge);
        }

        public static void Subscribe(Type t, Entity g, Func<GameEvent, GameEvent> f, float priority = 0)
        {
            GameState.World!.Events.Subscribe(t, g, f, priority: priority);
        }

        public static void Subscribe<T>(Entity g, Func<GameEvent, GameEvent> f, float priority = 0) where T : GameEvent
        {
            GameState.World!.Events.Subscribe<T>(g, f, priority: priority);
        }


        public static void PushMessage(ColoredText ct)
        {
            GameState.World!.GetState<GameLog>().PushMessage(ct);
        }

        public static T GetState<T>() where T : StateHandler, new()
        {
            return GameState.World!.GetState<T>();
        }
    }
}
