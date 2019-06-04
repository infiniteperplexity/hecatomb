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
        public List<string> Prerequisites;

        public Research(string s) : base(s)
        {
            Prerequisites = new List<string>();
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


        // hardness 1
        public static Research FlintTools = new Research("FlintTools")
        {
            Name = "flint tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } }
        };

        // hardness 2
        public static Research BronzeTools = new Research("BronzeTools")
        {
            Name = "bronze tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "BronzeIngots", 2 }, { "Wood", 2 }, { "Flint", 2 } },
            Prerequisites = new List<string>() { "FlintTools", "Bronzeworking" }
        };

        // hardness 3
        public static Research SteelTools = new Research("SteelTools")
        {
            Name = "steel tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "SteelIngots", 2 }, { "Wood", 2 }, { "BronzeIngots", 2 } },
            Prerequisites = new List<string>() { "BronzeTools", "Steelworking" }
        };

        // hardness 4
        public static Research AlloyTools = new Research("AlloyTools")
        {
            Name = "alloy tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "AlloyIngots", 2 }, { "Wood", 2 }, { "SteelIngots", 2 } },
            Prerequisites = new List<string>() { "SteelTools", "AlloySteel" }
        };

        public static Research SpearTrap = new Research("SpearTrap")
        {
            Name = "spear trap",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 1 }, { "Wood", 1 } }
        };

        public static Research BronzeWorking = new Research("Bronzeworking")
        {
            Name = "bronzeworks",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "BronzeOre", 2 }, { "Coal", 2 }, { "Stone", 2 } },
            Prerequisites = new List<string>() { "FlintTools" }
        };

        public static Research IronWorking = new Research("Steelworking")
        {
            Name = "ironworks",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "IronOre", 4 }, { "Coal", 4 }, { "BronzeIngots", 2 } },
            Prerequisites = new List<string>() { "BronzeTools" }
        };

        public static Research AlloySteel = new Research("AlloySteel")
        {
            Name = "alloy steel",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } },
            Prerequisites = new List<string>() { "SteelTools" }
        };

        public static Research Chirurgy = new Research("Chirurgy")
        {
            Name = "chirurgy",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flesh", 2 }, { "Bone", 2 }, { "Flint", 2 } },
            Prerequisites = new List<string>() { "BronzeTools" }
        };

        public static Research Masonry = new Research("Masonry")
        {
            Name = "masonry",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Rock", 2 }, { "Flint", 2 }, { "Wood", 2 } },
            Prerequisites = new List<string>() { "BronzeTools" }
        };

        // or should this be palladium / iridium?
        public static Research AdamantSteel = new Research("AdamantSteel")
        {
            Name = "adamant steel",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } }
        };

        public static Research ThoriumSteel = new Research("ThoriumSteel")
        {
            Name = "thorium steel",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } }
        };

        public static Research CondenseEctoplasm = new Research("CondenseEctoplasm")
        {
            Name = "condense ectoplasm",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flesh", 1 }, { "Bone", 1 } }
        };

        public static Research LongShadow = new Research("LongShadow")
        {
            Name = "long shadow",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Ectoplasm", 2 }}
        };

        public static Research PoundOfFlesh = new Research("PoundOfFlesh")
        {
            Name = "pound of flesh",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flesh", 2 }, { "Ectoplasm", 1 } }
        };


    }
}
