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
    public class Forge : Structure
    {
        public Forge() : base()
        {
            Symbols = new char[]
            {
                '\u26EC','.','\u2692',
                '\u2604','.','\u25A7',
                '\u26CF','=','.'
            };
            FGs = new string[]
            {
                "#99FF33", "FLOORFG","WALLFG",
                "#FF5500", "FLOORFG","#BB9922",
                "WALLFG","#FF9933","FLOORFG"
            };
            BG = "#554488";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"TinOre", 1}},
                new Dictionary<string, int>() {{"Wood", 1}}, new Dictionary<string, int>() {{"Coal", 2}}, new Dictionary<string, int>() {{"Wood", 1}},
                new Dictionary<string, int>() {{"CopperOre", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            MenuName = "forge";
            Name = "forge";
            UseHint = "(research metal tools and weapons.)";
            //Researches = new[] { "Bronze };
            Researches = new[] { "Bronzeworking" };
            //ResearchPrereqs = new[] { "Bronzeworking"};
            StructurePrereqs = new[] { "Workshop" };
        }
    }
}
