/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 10:24 AM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
    /// <summary>
    /// Description of GuardPost.
    /// </summary>
    public class BlackMarket : Structure
    {
        public BlackMarket() : base()
        {
            Symbols = new char[]
            {
                '\u00A3','.','.',
                '.','\u2696','$',
                '\u00A2','\u20AA','\u00A4'
            };
            FGs = new string[]
            {
                "#552222", "FLOORFG","FLOORFG",
                "FLOORFG", "#888844","#888844",
                "#225522","#333333","#222266"
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
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, new Dictionary<string, int>() {{"TradeGoods", 1}}, null,
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            MenuName = "black market";
            Name = "black market";
        }
    }
}
