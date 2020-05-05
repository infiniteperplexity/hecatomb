using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class AchievementHandler : StateHandler, IDisplayInfo
    {

        public List<string> Achieved = new List<string>();
        [JsonIgnore] public List<Achievement> Achievements;

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
                },
                new Achievement()
                {
                    Name = "Tools Of The Trade.",
                    Description = "(research flint tools at a workshop.)",
                    Condition = (AchievementEvent a) => {return a.Action=="ResearchFlintTools"; }
                },
                new Achievement()
                {
                    Name = "Limb From Limb.",
                    Description = "(build a slaughterhouse, butcher a corpse for flesh and bone.)",
                    Condition = (AchievementEvent a) => {return a.Action=="ButcherCorpse"; }
                },
                new Achievement()
                {
                    Name = "Fully Stocked.",
                    Description = "(store four different resources in a stockpile.)",
                    Condition = (AchievementEvent a) => {return a.Action=="FullyStocked"; }
                },
                new Achievement()
                {
                    Name = "Army Of The Dead.",
                    Description = "(build a sanctum to raise a fourth zombie.)",
                    Condition = (AchievementEvent a) => {return a.Action=="RaiseFourthZombie"; }
                },
                new Achievement()
                {
                    Name = "Striking Gold.",
                    Description = "(find some gold.)",
                    Condition = (AchievementEvent a) => {return a.Action=="FoundGold"; }
                },
                new Achievement()
                {
                    Name = "Let's Make A Deal.",
                    Description = "(finish a trade at the black market.)",
                    Condition = (AchievementEvent a) => {return a.Action=="FinishedTrade"; }
                },
                new Achievement()
                {
                    Name = "A Splash Of Color.",
                    Description = "(build an apothecary, find flowers, and dye a fixture.)",
                    Condition = (AchievementEvent a) => {return a.Action=="FinishedDyeTask"; }
                },
            };
        }


        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            menu.KeyMap[Keys.Z] = Commands.ChooseSpell;
            menu.KeyMap[Keys.J] = Commands.ChooseTask;
            menu.KeyMap[Keys.R] = Commands.ShowResearch;
            menu.KeyMap[Keys.L] = Commands.ShowLog;
            menu.KeyMap[Keys.V] = Commands.ShowAchievements;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            //var list = new List<ColoredText>() { "{magenta}Achievements:" };
            menu.InfoTop.Add("{magenta}Achievements:");
            foreach (var achieve in Achievements)
            {
                if (Achieved.Contains(achieve.Name))
                //if (achieve.Unlocked)
                {
                    menu.InfoTop.Add("{magenta}- " + achieve.Name + ": " + achieve.Description);
                }
                else
                {
                    menu.InfoTop.Add("- " + achieve.Name + ": " + achieve.Description);
                }
            }
            //return list;
        }
        public GameEvent HandleEvent(GameEvent g)
        {
            AchievementEvent ae = (AchievementEvent)g;
            foreach (Achievement achievement in Achievements)
            {
                if (achievement.Condition(ae) && !Achieved.Contains(achievement.Name))
                //if (achievement.Condition(ae) && !achievement.Unlocked)
                {
                    PushMessage("{magenta}Achievement unlocked: " + achievement.Name + " " + achievement.Description);
                    //achievement.Unlocked = true;
                    Achieved.Add(achievement.Name);
                }
            }
            return g;
        }


        public class Achievement
        {
            public string Name;
            public string Description;
            //public bool Unlocked;
            [JsonIgnore] public Func<AchievementEvent, bool> Condition;
            public Achievement()
            {
                Name = "Default";
                Description = "Should be overwritten.";
                Condition = Nothing;
            }
            public bool Nothing(AchievementEvent a)
            {
                return false;
            }
        }
    }
}
