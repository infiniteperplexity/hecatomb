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
    using static Research;
    public class Library : Structure
    {
        public Library() : base()
        {
            Symbols = new char[]
            {
                '\u25A4','.','\u25A4',
                '\u25A4','.','\u25A4',
                '\u25A4','.','\u25A4'
            };
            FGs = new string[]
            {
                "#BB44BB", "FLOORFG","#BB4444",
                "#BBBB44", "FLOORFG","#4444BB",
                "#44BB44", "FLOORFG","#44BBBB"
            };
            _bg = "#442244";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Gold, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Wood, 1}},
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Ectoplasm, 1}}, new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>() {{Wood, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Gold, 1}}
            };
            _name = "library";
            // should it add Sanity as well?
            UseHint = "(research new technologies.)";
            Researches = new[] { BronzeWeapons, AlloySteel };
            RequiresStructures = new[] { typeof(BlackMarket) };
        }
    }
}
