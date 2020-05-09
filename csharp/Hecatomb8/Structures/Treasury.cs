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
    using static Resource;
    public class Treasury : Structure
    {
        public Treasury() : base()
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
            _bg = "#555544";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{CopperOre, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>(),new Dictionary<Resource, int>() {{Flint, 1}},
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Wood, 1}},new Dictionary<Resource, int>() {{Rock, 1}},new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}},new Dictionary<Resource, int>() {{Wood, 1}},new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>() {{Flint, 1}},new Dictionary<Resource, int>(),new Dictionary<Resource, int>(),new Dictionary<Resource, int>() {{TinOre, 1}}
            };
            _name = "treasury";
            UseHint = "(store valuable resources.)";
            StoresResources = new Resource[] { Gold, Silk/*, BronzeIngots, SteelIngots, AlloyIngots, ThoriumIngots, AdamantIngots*/ };
            RequiresStructures = new[] { typeof(Stockpile), typeof(BlackMarket) };
        }
    }
}
