/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 9/18/2018
 * Time: 9:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Hecatomb
{
	/// <summary>
	/// Descripti
	/// </summary>
	public class GameColors
	{
		
		private Dictionary<string, Color> colors;

		public GameColors()
		{
			colors = new Dictionary<string, Color>() {
				{"white", Color.White},
				{"gray", Color.Gray},
				{"dark gray", new Color(0x55, 0x55, 0x55)},
				{"black", Color.Black},
				{"magenta", Color.Magenta},
				{"green", Color.LightGreen}
			};
		}


	   	public Color this[string s]
	   	{
	      get {
				Color result;
				if (colors.TryGetValue(s, out result)) {
					return result;
				} else {
					return Color.Red;
				}
			}
	      set { colors[s] = value; }
	   	}
	}
	  
}
