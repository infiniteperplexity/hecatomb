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
            BG = "#444455";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                null, null, null, null,
                null,new Dictionary<string, int>() {{"Wood", 1}},new Dictionary<string, int>() {{"Rock", 1}},null,
                null,new Dictionary<string, int>() {{"Rock", 1}},new Dictionary<string, int>() {{"Wood", 1}},null,
                null,null,null,null
                //new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}},
                //null, new Dictionary<string, int>() {{"Wood", 1}}, null,
                //new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}}
            };
            MenuName = "stockpile";
            Name = "stockpile";
            Stores = new string[] { "Rock", "Wood", "Flint", "Coal"};
        }
    }
}
