/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 9:53 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Player.
	/// </summary>
	public class Player
	{
		public int x {get; set;}
		public int y {get; set;}
		public char sym {get; set;}
		public Player()
		{
			x = 1;
			y = 1;
			sym = '@';
		}
	}
}
