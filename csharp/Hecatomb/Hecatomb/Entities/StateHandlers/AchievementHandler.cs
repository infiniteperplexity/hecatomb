using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb
{
    public class AchievementHandler : StateHandler
    {
        public List<Achievement> Achievements;

        public AchievementHandler() : base()
        {
            AddListener<AchievementEvent>(HandleEvent);
            Achievements = new List<Achievement>()
            {
                new Achievement()
                {
                    Name = "Bring Out Your Dead!",
                    Description = "(raise one zombie.)",
                    Condition = (AchievementEvent a) => {return a.Action=="CastRaiseZombie"; }
                }
            };
        }
        public GameEvent HandleEvent(GameEvent g)
        {
            AchievementEvent ae = (AchievementEvent)g;
            foreach (Achievement achievement in Achievements)
            {
                if (achievement.Condition(ae) && !achievement.Unlocked)
                {
                    Game.StatusPanel.PushMessage("{magenta}Achievement unlocked: " + achievement.Name + " " + achievement.Description);
                    achievement.Unlocked = true;
                }
            }
            return g;
        }


        public class Achievement
        {
            public string Name;
            public string Description;
            public bool Unlocked;
            [JsonIgnore] public Func<AchievementEvent, bool> Condition;
            public Achievement()
            {
                Condition = Nothing;
            }
            public bool Nothing(AchievementEvent a)
            {
                return false;
            }
        }
    }
}
