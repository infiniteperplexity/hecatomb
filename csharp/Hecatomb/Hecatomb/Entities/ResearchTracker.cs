using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hecatomb
{
    public class ResearchTracker : StateTracker
    {
        public List<string> Researched;
        public ResearchTracker() : base()
        {
            Researched = new List<string>();
        }

        public override void Activate()
        {
            base.Activate();
        }
    }


    public class ResearchMenuListing : IMenuListable
    {
        Research MyResearch;
        Structure MyStructure;
        public ResearchMenuListing(Research r, Structure s)
        {
            MyResearch = r;
            MyStructure = s;
        }

        public ColoredText ListOnMenu()
        {
            return MyResearch.Name;
        }

        public void ChooseFromMenu()
        {
            MyStructure.Researching = MyResearch.Name;
            MyStructure.ResearchTurns = MyResearch.Turns;
        }
    }
    public class Research : FlyWeight<Research>
    {
        public string Name;
        public Dictionary<string, int> Ingredients;
        public int Turns;

        public Research(string s) : base(s)
        {
       
        }

        public bool Researched
        {
            get
            {
                return Game.World.GetTracker<ResearchTracker>().Researched.Contains(Name);
            }
            set
            {
                if (value==false)
                {
                    Game.World.GetTracker<ResearchTracker>().Researched.Remove(Name);
                } 
                else if (value==true)
                {
                    Game.World.GetTracker<ResearchTracker>().Researched.Add(Name);
                }
            }
        }

       

        public static Research FlintTools = new Research("FlintTools")
        {
            Name = "flint tools", Turns = 25, Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, {"Wood", 2 } }
        };

        public static Research SpearTrap = new Research("SpearTrap")
        {
            Name = "spear trap",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 1 }, { "Wood", 1 } }
        };


    }
}
