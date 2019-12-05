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
            BG = "#444455";
            BGs = new string[]
            {
                "#BB1100","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","#BB1100",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Flint", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, null, null,
                new Dictionary<string, int>() {{"Coal", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            MenuName = "slaughterhouse";
            Name = "slaughterhouse";
            UseHint = "(butcher corpses and store flesh and bone.)";
            Stores = new string[] { "Flesh", "Bone" };
        }
    }
}
