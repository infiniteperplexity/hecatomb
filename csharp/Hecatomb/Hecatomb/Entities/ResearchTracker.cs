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

    public class Research
    {
        public string Name;
        public Dictionary<string, int> Ingredients;
        public int Turns;

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

        public static Research FlintTools = new Research()
        {
            Name = "FlintTools", Turns = 25, Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, {"Wood", 2 } }
        };

    }
}
