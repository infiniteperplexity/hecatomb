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
        //public static CommandsPanel Menu
        //{
        //    get
        //    {
        //        return Game.MenuPanel;
        //    }
        //}
        //public static InformationPanel Status
        //{
        //    get
        //    {
        //        return Game.InfoPanel;
        //    }
        //}

        //public static SplashPanel Foreground
        //{
        //    get
        //    {
        //        return Game.SplashPanel;
        //    }
        //}

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

        // World state
        //public static HecatombOptions Options
        //{
        //    get
        //    {
        //        return Game.Options;
        //    }
        //}

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
        //public static HashSet<Coord> Explored
        //{
        //    get
        //    {
        //        return Game.World.Explored;
        //    }
        //}
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
        //public static SparseJaggedArray3D<Particle> Particles
        //{
        //    get
        //    {
        //        return Game.World.Particles;
        //    }
        //}
        //public static AchievementHandler Achievements
        //{
        //    get
        //    {
        //        return Game.World.GetState<AchievementHandler>();
        //    }
        //}
        public static Dictionary<int, Entity> Entities
        {
            get
            {
                return GameState.World!.Entities;
            }
        }
        //public static ResearchHandler Research
        //{
        //    get
        //    {
        //        return Game.World.GetState<ResearchHandler>();
        //    }
        //}
        //public static TutorialHandler Tutorial
        //{
        //    get
        //    {
        //        return Game.World.GetState<TutorialHandler>();
        //    }
        //}
        //public static TurnHandler Turns
        //{
        //    get
        //    {
        //        return GameState.World!.Turns;
        //    }
        //}
        //public static TimeHandler Time
        //{
        //    get
        //    {
        //        return Game.Time;
        //    }
        //}

        // spawning methods
        
        //public static T GetState<T>() where T : StateHandler, new()
        //{
        //    return Game.World.GetState<T>();
        //}
    }
}
