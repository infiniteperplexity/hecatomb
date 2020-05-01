using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource;
    public class Research : FlyWeight<Research>
    {
        [JsonIgnore] public string Name;
        [JsonIgnore] public JsonArrayDictionary<Resource, int> Ingredients;
        [JsonIgnore] public int Turns;
        [JsonIgnore] public List<Research> RequiresResearch;

        public Research(string s) : base(s)
        {
            RequiresResearch = new List<Research>();
            Ingredients = new JsonArrayDictionary<Resource, int>();
        }

        //public bool Researched
        //{
        //    get
        //    {
        //        return Game.World.GetState<ResearchHandler>().Researched.Contains(Name);
        //    }
        //    set
        //    {
        //        if (value == false)
        //        {
        //            Game.World.GetState<ResearchHandler>().Researched.Remove(Name);
        //        }
        //        else if (value == true)
        //        {
        //            Game.World.GetState<ResearchHandler>().Researched.Add(Name);
        //        }
        //    }
        //}


        // hardness 1
        public static Research FlintTools = new Research("FlintTools")
        {
            Name = "flint tools",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Flint, 2 }, { Resource.Wood, 2 } }
        };

        public static Research BoneWeapons = new Research("BoneWeapons")
        {
            Name = "bone weapons",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Bone, 2 }, { Wood, 2 } }
        };

        // hardness 4
        public static Research AlloyTools = new Research("AlloyTools")
        {
            Name = "alloy tools",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { TitaniumOre, 2 }, { CobaltOre, 2 }, { Wood, 2 }, { IronOre, 2 }, { Coal, 6 } },
            RequiresResearch = new List<Research>() { SteelTools, AlloySteel }
        };

        public static Research SpearTrap = new Research("SpearTrap")
        {
            Name = "spear trap",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flint, 1 }, { Wood, 1 } }
        };

        public static Research BronzeWorking = new Research("Bronzeworking")
        {
            Name = "bronzeworks",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { CopperOre, 2 }, { TinOre, 2 }, { Coal, 2 }, { Rock, 2 } },
            RequiresResearch = new List<Research>() { FlintTools }
        };

        // hardness 2
        public static Research BronzeTools = new Research("BronzeTools")
        {
            Name = "bronze tools",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { CopperOre, 2 }, { TinOre, 2 }, { Coal, 2 }, { Wood, 2 }, { Flint, 2 } },
            RequiresResearch = new List<Research>() { FlintTools, BronzeWorking }
        };

        public static Research BronzeWeapons = new Research("BronzeWeapons")
        {
            Name = "bronze weapons",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { CopperOre, 2 }, { TinOre, 2 }, { Coal, 2 }, { Wood, 2 } }
        };

        public static Research IronWorking = new Research("Ironworking")
        {
            Name = "ironworks",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { IronOre, 4 }, { Coal, 4 }, { CopperOre, 2 }, { TinOre, 2 } },
            RequiresResearch = new List<Research>() { BronzeTools }
        };

        // hardness 3
        public static Research SteelTools = new Research("SteelTools")
        {
            Name = "steel tools",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { IronOre, 2 }, { Coal, 2 }, { Wood, 4 }, { CopperOre, 2 }, { TinOre, 2 } },
            RequiresResearch = new List<Research>() { BronzeTools, IronWorking }
        };


        public static Research AlloySteel = new Research("AlloySteel")
        {
            Name = "alloy steel",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { TitaniumOre, 2 }, { CobaltOre, 2 }, { Coal, 6 }, { IronOre, 4 } },
            RequiresResearch = new List<Research>() { SteelTools }
        };

        public static Research Chirurgy = new Research("Chirurgy")
        {
            Name = "chirurgy",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flesh, 2 }, { Bone, 2 }, { Flint, 2 }, { Ectoplasm, 1 } },
            RequiresResearch = new List<Research>() { FlintTools, CondenseEctoplasm }
        };

        public static Research Masonry = new Research("Masonry")
        {
            Name = "masonry",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Rock, 2 }, { Flint, 2 }, { Wood, 2 } },
            RequiresResearch = new List<Research>() { BronzeTools }
        };

        // or should this be palladium / iridium?
        public static Research AdamantSteel = new Research("AdamantSteel")
        {
            Name = "adamant steel",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flint, 2 }, { Wood, 2 } }
        };

        public static Research ThoriumSteel = new Research("ThoriumSteel")
        {
            Name = "thorium steel",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flint, 2 }, { Wood, 2 } }
        };

        public static Research CondenseEctoplasm = new Research("CondenseEctoplasm")
        {
            Name = "condense ectoplasm",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flesh, 1 }, { Bone, 1 } }
        };

        public static Research ShadowHop = new Research("ShadowHop")
        {
            Name = "shadow hop",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Ectoplasm, 2 } }
        };

        public static Research SiphonFlesh = new Research("SiphonFlesh")
        {
            Name = "siphon flesh",
            Turns = 25,
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Flesh, 2 } }
        };
    }
}
