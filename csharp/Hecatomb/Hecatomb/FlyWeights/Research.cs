using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    using static HecatombAliases;

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
                return Game.World.GetState<ResearchHandler>().Researched.Contains(Name);
            }
            set
            {
                if (value == false)
                {
                    Game.World.GetState<ResearchHandler>().Researched.Remove(Name);
                }
                else if (value == true)
                {
                    Game.World.GetState<ResearchHandler>().Researched.Add(Name);
                }
            }
        }



        public static Research FlintTools = new Research("FlintTools")
        {
            Name = "flint tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } }
        };

        public static Research SpearTrap = new Research("SpearTrap")
        {
            Name = "spear trap",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 1 }, { "Wood", 1 } }
        };

        public static Research CondenseEctoplasm = new Research("CondenseEctoplasm")
        {
            Name = "condense ectoplasm",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flesh", 1 }, { "Bone", 1 } }
        };


    }
}
