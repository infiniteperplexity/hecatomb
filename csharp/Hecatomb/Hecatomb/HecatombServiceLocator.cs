using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public static class HecatombServiceLocator
    {
        // GUI elements
        public static MainGamePanel MainPanel;
        public static MenuGamePanel MenuPanel;
        public static StatusGamePanel StatusPanel;
        public static ControlContext Controls;
        public static GameCommands Commands;
        public static GameColors Colors;

        // World state
        public static GameWorld World;
        public static Player Player;
        public static Terrain[,,] Tiles;
        public static Cover[,,] Covers;
        public static SparseArray3D<Creature> Creatures;
        public static SparseArray3D<Feature> Features;
        public static SparseArray3D<Task> Tasks;
        public static SparseArray3D<Item> Items;
        public static SparseJaggedArray3D<Particle> Particles;
        public static AchievementTracker Achievements;
        public static EntityHandler Entities;
        public static ResearchTracker SomeResearch;
        public static TutorialTracker Tutorial;
        public static TurnHandler Turns;
        public static TimeHandler Time;

        


    }
}
