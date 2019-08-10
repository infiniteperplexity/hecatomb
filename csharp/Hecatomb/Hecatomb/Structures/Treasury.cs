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

                //'\u2554','\u2550','\u2550','\u2557',
                //'\u2551','.','.','\u2551',
                //'\u2551','.','.','\u2551',
                //'\u255A','\u2550','\u2550','\u255D'
                //'.','.','\u25AD',
                //'\u2234','#','\u2630',
                //'\u25A7','.','\u25AF'
            };
            FGs = new string[]
            {
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG",
                "FLOORFG","FLOORFG","FLOORFG","FLOORFG"
                //"#BB9922", "FLOORFG","FLOORFG",
                //"#BB9922","FLOORFG", "#BB9922",
                //"FLOORFG","FLOORFG","#BB9922"
            };
            BG = "#555544";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"CopperOre", 1}}, null, null, new Dictionary<string, int>() {{"Flint", 1}},
                null,new Dictionary<string, int>() {{"Wood", 1}},new Dictionary<string, int>() {{"Rock", 1}},null,
                null,new Dictionary<string, int>() {{"Rock", 1}},new Dictionary<string, int>() {{"Wood", 1}},null,
                new Dictionary<string, int>() {{"Flint", 1}},null,null,new Dictionary<string, int>() {{"TinOre", 1}}
            };
            MenuName = "treasury";
            Name = "treasury";
            Stores = new string[] { "TradeGoods", "BronzeIngots", "SteelIngots", "AlloyIngots", "ThoriumIngots", "AdamantIngots"};
            StructurePrereqs = new[] { "Stockpile", "BlackMarket" };
        }
    }
}
