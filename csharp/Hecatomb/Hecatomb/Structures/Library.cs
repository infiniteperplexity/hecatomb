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
            BG = "#442244";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"TradeGoods", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, new Dictionary<string, int>() {{"Ectoplasm", 1}}, null,
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"TradeGoods", 1}}
            };
            MenuName = "library";
            Name = "library";
            // should it add Sanity as well?
            UseHint = "(research new technologies.)";
            Researches = new[] { "Bronzeworking", "Chirurgy", "Masonry", "Steelworking", "AlloySteel" };
            StructurePrereqs = new[] { "BlackMarket" };
        }
    }
}
