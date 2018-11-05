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
using System.Reflection;
using System.Diagnostics;
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
				{"lime green", Color.LimeGreen},
				{"olive", Color.Olive},
				{"light gray", Color.LightGray},
				{"light cyan", Color.LightCyan},
				{"gainsboro", Color.Gainsboro},
				{"black", Color.Black},
				{"magenta", Color.Magenta},
				{"green", Color.LightGreen},
				{"orange", Color.Orange},
				{"yellow", Color.Yellow},
				{"cyan", Color.Cyan},
				{"brown", Color.Brown},
				{"GRASSFG", new Color(0x66, 0x88, 0x44)},
				{"GRASSBG", new Color(0x33, 0x44, 0x22)},
				{"WALLFG", new Color(0xAA, 0xAA, 0xAA)},
				{"FLOORFG", new Color(0x77, 0x77, 0x88)},
				{"BELOWFG", new Color(0x55, 0x33, 0x55)},
				{"SHADOWFG", new Color(0x33, 0x33, 0x55)},
				{"WALLBG", new Color(0x77, 0x77, 0x88)},
				{"FLOORBG", new Color(0x44, 0x55, 0x66)},
				{"BELOWBG", new Color(0x11, 0x11, 0x22)},
				{"TWOBELOWFG", new Color(0x22, 0x11, 0x22)},
                {"WATERFG", new Color(0x33, 0x88, 0xFF)},
                {"WATERBG", new Color(0x11, 0x44, 0xBB)}
            };
			
//			PropertyInfo[] properties = typeof(Color).GetProperties();
//			foreach(PropertyInfo property in properties)
//			{
//				if (property.DeclaringType.Equals(typeof(Color)))
//				{
//				    string Name = property.Name;
//					string name = Name.ToLower();
//					Debug.WriteLine(property.GetValue(typeof(Color)));
////					Color c = (Color) property.GetValue(typeof(Color));
////					colors[Name] = c;
////					colors[name] = c;	
//				}
//			}
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
