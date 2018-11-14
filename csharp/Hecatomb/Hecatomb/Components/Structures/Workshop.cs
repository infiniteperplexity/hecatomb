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
    public class Workshop : Structure
    {
        public Workshop() : base()
        {
            Symbols = new char[]
            {
                '\u25AE','/','\u2699',
                '\u2261','\u25AA','.',
                '\u2692','.','\u25A7'
            };
            FGs = new string[]
            {
                "#BB9922", "#BB9922","FLOORFG",
                "#BB9922", "#BB9922","FLOORFG",
                "FLOORFG","FLOORFG","#BB9922"
            };
            //"#665555"
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, new Dictionary<string, int>() {{"Rock", 1},{"Flint", 1}}, null,
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}}
            };
            MenuName = "workshop";
            Name = "workshop";
        }
    }
}
