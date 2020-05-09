using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource;
    public class Stockpile : Structure
    {
        public Stockpile() : base()
        {
            Width = 4;
            Height = 4;
            Symbols = new char[]
            {
                '#','-','-','#',
                '|','.','.','|',
                '|','.','.','|',
                '#','-','-','#'
            };
            FGs = new string[]
            {
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG"
            };
            _bg = "#444455";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(),new Dictionary<Resource, int>() {{Wood, 1}},new Dictionary<Resource, int>() {{Rock, 1}},new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(),new Dictionary<Resource, int>() {{Rock, 1}},new Dictionary<Resource, int>() {{Wood, 1}},new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>()
            };
            _name = "stockpile";
            UseHint = "(store common resources.)";
            StoresResources = new Resource[] { Rock, Wood, Flint, Coal, IronOre, TinOre, CopperOre };
        }
    }
}
