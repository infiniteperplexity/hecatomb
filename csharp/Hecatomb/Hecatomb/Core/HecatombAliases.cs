using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public static class HecatombAliases
    {
        // GUI elements
        public static MainGamePanel MainPanel
        {
            get
            {
                return Game.MainPanel;
            }
        }
        public static MenuGamePanel Menu
        {
            get
            {
                return Game.MenuPanel;
            }
        }
        public static StatusGamePanel Status
        {
            get
            {
                return Game.StatusPanel;
            }
        }
        public static ControlContext Controls
        {
            get
            {
                return Game.Controls;
            }
        }
        public static Commands Commands
        {
            get
            {
                return Game.Commands;
            }
        }
        public static Colors Colors
        {
            get
            {
                return Game.Colors;
            }
        }

        // World state
        public static World World
        {
            get
            {
                return Game.World;
            }
        }
        public static Player Player
        {
            get
            {
                return Game.World.Player;
            }
        }
        public static Terrain[,,] Terrains
        {
            get
            {
                return Game.World.Terrains;
            }
        }
        public static Cover[,,] Covers
        {
            get
            {
                return Game.World.Covers;
            }
        }
        public static SparseArray3D<Creature> Creatures
        {
            get
            {
                return Game.World.Creatures;
            }
        }
        public static SparseArray3D<Feature> Features
        {
            get
            {
                return Game.World.Features;
            }
        }
        //public static SparseArray3D<Task> Tasks
        public static SparseArray3D<TaskEntity> Tasks
        {
            get
            {
                return Game.World.Tasks;
            }
        }
        public static SparseArray3D<Item> Items
        {
            get
            {
                return Game.World.Items;
            }
        }
        public static SparseJaggedArray3D<Particle> Particles
        {
            get
            {
                return Game.World.Particles;
            }
        }
        public static AchievementHandler Achievements
        {
            get
            {
                return Game.World.GetTracker<AchievementHandler>();
            }
        }
        public static Dictionary<int, Entity> Entities
        {
            get
            {
                return Entity.Entities;
            }
        }
        public static ResearchTracker Research
        {
            get
            {
                return Game.World.GetTracker<ResearchTracker>();
            }
        }
        public static TutorialHandler Tutorial
        {
            get
            {
                return Game.World.GetTracker<TutorialHandler>();
            }
        }
        public static TurnHandler Turns
        {
            get
            {
                return Game.World.Turns;
            }
        }
        public static TimeHandler Time
        {
            get
            {
                return Game.Time;
            }
        }
    }
}
