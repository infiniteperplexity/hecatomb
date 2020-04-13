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
        public static MainPanel MainPanel
        {
            get
            {
                return OldGame.MainPanel;
            }
        }
        public static CommandsPanel Menu
        {
            get
            {
                return OldGame.MenuPanel;
            }
        }
        public static InformationPanel Status
        {
            get
            {
                return OldGame.InfoPanel;
            }
        }

        public static SplashPanel Foreground
        {
            get
            {
                return OldGame.SplashPanel;
            }
        }

        public static ControlContext Controls
        {
            get
            {
                return OldGame.Controls;
            }
        }
        public static HecatombCommmands Commands
        {
            get
            {
                return OldGame.Commands;
            }
        }
        public static Colors Colors
        {
            get
            {
                return OldGame.Colors;
            }
        }

        // World state
        public static HecatombOptions Options
        {
            get
            {
                return OldGame.Options;
            }
        }

        public static World World
        {
            get
            {
                return OldGame.World;
            }
        }
        public static Creature Player
        {
            get
            {
                return OldGame.World.Player;
            }
        }
        public static HashSet<Coord> Explored
        {
            get
            {
                return OldGame.World.Explored;
            }
        }
        public static Terrain[,,] Terrains
        {
            get
            {
                return OldGame.World.Terrains;
            }
        }
        public static Cover[,,] Covers
        {
            get
            {
                return OldGame.World.Covers;
            }
        }
        public static SparseArray3D<Creature> Creatures
        {
            get
            {
                return OldGame.World.Creatures;
            }
        }
        public static SparseArray3D<Feature> Features
        {
            get
            {
                return OldGame.World.Features;
            }
        }
        public static SparseArray3D<Task> Tasks
        {
            get
            {
                return OldGame.World.Tasks;
            }
        }
        public static SparseArray3D<Item> Items
        {
            get
            {
                return OldGame.World.Items;
            }
        }
        public static SparseJaggedArray3D<Particle> Particles
        {
            get
            {
                return OldGame.World.Particles;
            }
        }
        public static AchievementHandler Achievements
        {
            get
            {
                return OldGame.World.GetState<AchievementHandler>();
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
                return OldGame.World.GetState<ResearchHandler>();
            }
        }
        public static TutorialHandler Tutorial
        {
            get
            {
                return OldGame.World.GetState<TutorialHandler>();
            }
        }
        public static TurnHandler Turns
        {
            get
            {
                return OldGame.World.Turns;
            }
        }
        public static TimeHandler Time
        {
            get
            {
                return OldGame.Time;
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

        public static T GetState<T>() where T : StateHandler, new()
        {
            return OldGame.World.GetState<T>();
        }
    }
}
