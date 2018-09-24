/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 9/18/2018
 * Time: 9:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Hecatomb
{
	/// <summary>
	/// Descripti
	/// </summary>
	public class Colors
	{
		private Dictionary<string, Microsoft.Xna.Framework.Color> colorMap;

		public Colors() {
			colorMap = new Dictionary<string, Microsoft.Xna.Framework.Color>();
			colorMap["white"] = Microsoft.Xna.Framework.Color.White;
			colorMap["black"] = Microsoft.Xna.Framework.Color.Black;
			colorMap["magenta"] = Microsoft.Xna.Framework.Color.Magenta;
			colorMap["green"] = Microsoft.Xna.Framework.Color.LightGreen;
		}
		
		
		public Microsoft.Xna.Framework.Color this[string s]
		{
			get {
				Microsoft.Xna.Framework.Color result;
				if (colorMap.TryGetValue(s, out result)) {
					return result;
				} else {
					return Microsoft.Xna.Framework.Color.Red;
				}
			}
			set {
				// if this isn't static, we're going to want it to be settable
			}
		}
	}
}
