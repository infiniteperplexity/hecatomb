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
				'#',' ','#',
				' ','0',' ',
				'#',' ','#'
			};
			FGs = new string[]
			{
				"WALLFG","FLOORFG","WALLFG",
				"FLOORFG","WALLFG","FLOORFG",
				"WALLFG","FLOORFG","WALLFG",
			};
			BGs = new string[]
			{
				"WALLBG","FLOORBG","WALLBG",
				"FLOORBG","WALLBG","FLOORBG",
				"WALLBG","FLOORBG","WALLBG",
			};
            Ingredients = new Dictionary<string, int>[]
            {
                null, null, null,
                null, new Dictionary<string, int>() {{"Rock", 1}}, null,
                null, null, null
            };
			MenuName = "guard post";
			Name = "guard post";
		}
	}
}
