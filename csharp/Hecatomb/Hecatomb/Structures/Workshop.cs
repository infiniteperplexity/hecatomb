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
            Width = 2;
            Height = 2;
            Symbols = new char[]
            {
                '.','\u2699',
                '\u2692','\u25A7'
                //'\u25AE','/','\u2699',
                //'\u2261','\u25AA','.',
                //'\u2692','.','\u25A7'
            };
            FGs = new string[]
            {
                "FLOORFG","#CCCCCC",
                "#CCCCCC","#BB9922"
                //"#BB9922", "#BB9922","#CCCCCC",
                //"#BB9922", "#BB9922","FLOORFG",
                //"#CCCCCC","FLOORFG","#BB9922"
            };
            BG = "#665555";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Rock", 1}}, new Dictionary<string, int>() {{"Flint", 1}},
                new Dictionary<string, int>() {{"Rock", 1}, {"Wood", 2}}, new Dictionary<string, int>() {{"Wood", 2}}
                //new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                //null, new Dictionary<string, int>() {{"Rock", 1},{"Flint", 1}}, null,
                //new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}}
            };
            Harvests = new Dictionary<string, float>[]
            {
                new Dictionary<string, float>() , new Dictionary<string, float>() {{"Flint", 1}},
                new Dictionary<string, float>() , new Dictionary<string, float>()
                //new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                //null, new Dictionary<string, int>() {{"Rock", 1},{"Flint", 1}}, null,
                //new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}}
            };
            MenuName = "workshop";
            Name = "workshop";
            UseHint = "(enables furnish task; research basic tools and weapons.)";
            Researches = new[] { "FlintTools", "BoneWeapons", "SpearTrap", "BronzeTools", "SteelTools", "AlloyTools" };
        }
    }
}
