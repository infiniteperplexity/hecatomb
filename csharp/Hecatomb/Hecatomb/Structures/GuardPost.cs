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
	public class GuardPost : Structure
	{
		public GuardPost() : base()
		{
			Symbols = new char[]
			{
                '\u2694','.','\u2658',
				'.','.','.',
                '\u2658','.','\u2694'
            };
			FGs = new string[]
			{
				"WALLFG","FLOORFG","WALLFG",
				"FLOORFG","WALLFG","FLOORFG",
				"WALLFG","FLOORFG","WALLFG",
			};
            BG = "#555577";
            BGs = new string[]
			{
				"WALLBG","FLOORBG","WALLBG",
				"FLOORBG","FLOORBG","FLOORBG",
				"WALLBG","FLOORBG","WALLBG",
			};
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Wood", 1}}, null, null,
                null, new Dictionary<string, int>() {{"Rock", 1}}, null,
                null, null, new Dictionary<string, int>() {{"Wood", 1}}
            };
			MenuName = "guard post";
			Name = "guard post";
		}
	}
}
