/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 10:24 AM
 */
using System;

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
			MenuName = "guard post";
			Name = "guard post";
		}
	}
}
