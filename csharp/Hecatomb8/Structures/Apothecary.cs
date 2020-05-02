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
    using static Research;
    using static Resource;
    public class Apothecary : Structure
    {
        public Apothecary() : base()
        {
            Symbols = new char[]
            {
                '\u264B','.','\u264F',
                '\u264C','.','\u2651',
                '.', '\u264D','\u2653'
            };
            FGs = new string[]
            {
                "#BB44BB", "FLOORFG","#BB4444",
                "#BBBB44", "FLOORFG","#9999DD",
                "FLOORFG", "#44BB44","#44BBBB"
            };
            _bg = "#335511";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Ectoplasm, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Wood, 1}},
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Gold, 1}}, new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>() {{Wood, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Ectoplasm, 1}}
            };
            _name = "apothecary";
            UseHint = "(stores dyes and enables dyeing)";
            RequiresStructures = new[] { typeof(BlackMarket) };
            //Stores = new string[RandomPaletteHandler.FlowerNames.Count];
            //for (var i = 0; i < RandomPaletteHandler.FlowerNames.Count; i++)
            //{
            //    Stores[i] = RandomPaletteHandler.FlowerNames[i].Item1;
            //}
        }
    }
}
