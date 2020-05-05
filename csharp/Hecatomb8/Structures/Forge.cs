using System;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static Resource;
    using static Research;
    public class Forge : Structure
    {
        
        public Forge() : base()
        {
            Symbols = new char[]
            {
                '\u26EC','.','\u2692',
                '\u2604','.','\u25A7',
                '\u26CF','=','.'
            };
            FGs = new string[]
            {
                "#99FF33", "FLOORFG","WALLFG",
                "#FF5500", "FLOORFG","#BB9922",
                "WALLFG","#FF9933","FLOORFG"
            };
            _bg = "#554488";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{TinOre, 1}},
                new Dictionary<Resource, int>() {{Wood, 1}}, new Dictionary<Resource, int>() {{Coal, 2}}, new Dictionary<Resource, int>() {{Wood, 1}},
                new Dictionary<Resource, int>() {{CopperOre, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}}
            };
            _name = "forge";
            UseHint = "(research metal tools and weapons.)";
            Researches = new[] { BronzeWorking };
            RequiresStructures = new[] { typeof(Workshop) };
        }
    }
}
