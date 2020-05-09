using System;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static Resource;
    using static Research;
    public class StoneMason : Structure
    {
        public StoneMason() : base()
        {
            Symbols = new char[]
            {
                '\u2625','.','\u2AEF',
                '.','\u2135','.',
                '\u2AEF','\u2606','\u263F'
            };
            FGs = new string[]
            {
                "magenta", "FLOORFG","cyan",
                "FLOORFG", "green","FLOORFG",
                "yellow","red","orange"
            };
            _bg = "#222244";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}},
                new Dictionary<Resource, int>() {{Wood, 1}}, new Dictionary<Resource, int>() {{Coal, 2}}, new Dictionary<Resource, int>() {{Wood, 1}},
                new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}}
            };
            _name = "stonemason";
            UseHint = "(currently does nothing.)";
            RequiresStructures = new[] { typeof(Workshop), typeof(Stockpile) };
            RequiresResearch = new Research[] { /*Masonry*/ };
        }
    }
}
