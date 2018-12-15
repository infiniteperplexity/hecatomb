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
        public static HecatombCommmands Commands
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
        public static HecatombOptions Options
        {
            get
            {
                return Game.Options;
            }
        }

        public static World World
        {
            get
            {
                return Game.World;
            }
        }
        public static PlayerEntity Player
        {
            get
            {
                return Game.World.Player;
            }
        }
        public static HashSet<Coord> Explored
        {
            get
            {
                return Game.World.Explored;
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
        public static SparseArray3D<Task> Tasks
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
        public static ResearchHandler Research
        {
            get
            {
                return Game.World.GetTracker<ResearchHandler>();
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

        // spawning methods
        public static Entity Spawn(Type t)
        {
            return Entity.Spawn(t);
        }
        public static T Spawn<T>() where T : Entity, new()
        {
            return Entity.Spawn<T>();
        }
        public static T Spawn<T>(Type t) where T : Entity
        {
            return Entity.Spawn<T>(t);
        }
        public static T Spawn<T>(string s) where T : TypedEntity, new()
        {
            return Entity.Spawn<T>(s);
        }

        public static T Mock<T>() where T : Entity, new()
        {
            return Entity.Mock<T>();
        }

        public static T Mock<T>(string s) where T : TypedEntity, new()
        {
            return Entity.Mock<T>(s);
        }

        public static Entity Mock(Type t)
        {
            return Entity.Mock(t);
        }

        public static T Mock<T>(Type t) where T : Entity
        {
            return Entity.Mock<T>(t);
        }
    }
}
