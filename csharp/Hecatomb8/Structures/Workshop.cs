using System;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource;
    using static Research;
    public class Workshop : Structure
    {

        public Workshop() : base()
        {
            Width = 2;
            Height = 2;
            Symbols = new char[]
            {
                '.','\u2699',
                '\u2692','\u25A7'
                //'\u25AE','/','\u2699',
                //'\u2261','\u25AA','.',
                //'\u2692','.','\u25A7'
            };
            FGs = new string[]
            {
                "FLOORFG","#CCCCCC",
                "#CCCCCC","#BB9922"
                //"#BB9922", "#BB9922","#CCCCCC",
                //"#BB9922", "#BB9922","FLOORFG",
                //"#CCCCCC","FLOORFG","#BB9922"
            };
            _bg = "#665555";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>() {{Flint, 1}},
                new Dictionary<Resource, int>() {{Rock, 1}, {Wood, 2}}, new Dictionary<Resource, int>() {{Wood, 2}}
            };
            Harvests = new Dictionary<Resource, float>[]
            {
                new Dictionary<Resource, float>() , new Dictionary<Resource, float>() {{Flint, 1}},
                new Dictionary<Resource, float>() , new Dictionary<Resource, float>()
            };
            //MockupName = "workshop";
            _name = "workshop";
            UseHint = "(enables furnish task; research basic tools and weapons.)";
            Researches = new[] { FlintTools, BoneWeapons, Research.SpearTrap, BronzeTools, SteelTools, AlloyTools };
        }
    }
}
