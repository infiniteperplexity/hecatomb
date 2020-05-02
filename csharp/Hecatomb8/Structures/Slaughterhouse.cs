using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource;
    public class Slaughterhouse : Structure
    {    
        public Slaughterhouse() : base()
        {

            Symbols = new char[]
            {
                '\u2694','.','%',
                '\u2234','.','\u2620',
                '\u25A7','.','\u25AA'
            };
            FGs = new string[]
            {
                "FLOORFG", "FLOORFG","FLOORFG",
                "#BB9922","FLOORFG", "FLOORFG",
                "FLOORFG","FLOORFG","#BB9922"
            };
            _bg = "#444455";
            BGs = new string[]
            {
                "#BB1100","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","#BB1100",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Flint, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>(){{Wood, 1}},
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(){{Coal, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}}
            };
            _name = "slaughterhouse";
            UseHint = "(enables butcher task and stores flesh and bone.)";
            StoresResources = new Resource[] { Flesh, Bone };
        }
    }
}
