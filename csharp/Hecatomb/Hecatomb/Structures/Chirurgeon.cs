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
    public class Chirurgeon : Structure
    {
        public Chirurgeon() : base()
        {
            Width = 2;
            Height = 2;
            Symbols = new char[]
            {
                '\u2625','%',
                '.','\u2694'
            };
            FGs = new string[]
            {
                "white", "pink",
                "FLOORFG", "#444499"
            };
            BG = "#777799";
            BGs = new string[]
            {
                "WALLBG","FLOORBG",
                "FLOORBG","FLOORBG"
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"TradeGoods", 1}}, new Dictionary<string, int>() {{"Bone", 2}},
                new Dictionary<string, int>() {{"Flesh", 2}}, new Dictionary<string, int>() {{"Ectoplasm", 1}}
            };
            MenuName = "chirurgeon";
            Name = "chirurgeon";
            UseHint = "(repair injured minions)";
            StructurePrereqs = new[] { "Slaughterhouse" };
            ResearchPrereqs = new[] { "Chirurgy" };
        }
    }
}
