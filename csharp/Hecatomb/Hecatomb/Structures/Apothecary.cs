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
    /// <summary>
    /// Description of GuardPost.
    /// </summary>
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
            BG = "#335511";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Ectoplasm", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, new Dictionary<string, int>() {{"Gold", 1}}, null,
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Ectoplasm", 1}}
            };
            MenuName = "apothecary";
            Name = "apothecary";
            UseHint = "(stores dyes and enables dyeing)";
            StructurePrereqs = new[] { "BlackMarket" };
            Stores = new string[RandomPaletteHandler.FlowerNames.Count];
            for (var i = 0; i < RandomPaletteHandler.FlowerNames.Count; i++)
            {
                Stores[i] = RandomPaletteHandler.FlowerNames[i].Item1;
            }
        }
    }
}
