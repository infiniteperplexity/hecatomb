/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 10:24 AM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
    /// <summary>
    /// Description of GuardPost.
    /// </summary>
    public class Stockpile : Structure
    {
        public Stockpile() : base()
        {

            Symbols = new char[]
            {
                '.','.','\u25AD',
                '\u2234','#','\u2630',
                '\u25A7','.','\u25AF'
            };
            FGs = new string[]
            {
                "#BB9922", "FLOORFG","FLOORFG",
                "#BB9922","FLOORFG", "#BB9922",
                "FLOORFG","FLOORFG","#BB9922"
            };
            BG = "#444455";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}},
                null, new Dictionary<string, int>() {{"Wood", 1}}, null,
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            MenuName = "stockpile";
            Name = "stockpile";
        }

        public override GameEvent OnTurnBegin(GameEvent ge)
        {
            Debug.WriteLine("Testing override for delegates");
            return ge;
        }
    }
}
