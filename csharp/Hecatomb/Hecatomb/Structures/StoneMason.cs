﻿/*
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
    public class StoneMason : Structure
    {
        public StoneMason() : base()
        {
            Symbols = new char[]
            {
                '\u2625','.','\u2AEF',
                '.','\u2135','.',
                '\u2AEF','\u2606','\u263F'
            };
            FGs = new string[]
            {
                "magenta", "FLOORFG","cyan",
                "FLOORFG", "green","FLOORFG",
                "yellow","red","orange"
            };
            BG = "#222244";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}},
                new Dictionary<string, int>() {{"Wood", 1}}, new Dictionary<string, int>() {{"Coal", 2}}, new Dictionary<string, int>() {{"Wood", 1}},
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            MenuName = "stonemason";
            Name = "stonemason";
            UseHint = "(currently does nothing.)";
            //Researches = new[] { "CondenseEctoplasm", "LongShadow", "PoundOfFlesh" };
            StructurePrereqs = new[] { "Workshop", "Stockpile" };
            ResearchPrereqs = new[] { "Masonry" };
        }
    }
}
