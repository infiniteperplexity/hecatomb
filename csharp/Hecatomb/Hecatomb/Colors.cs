/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 9/18/2018
 * Time: 9:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using RLNET;
using System.Collections.Generic;
using System;

namespace Hecatomb
{
	/// <summary>
	/// Descripti
	/// </summary>
	public class Colors
	{
		private Dictionary<string, RLColor> colorMap;

		public Colors() {
			colorMap = new Dictionary<string, RLColor>();
			colorMap["white"] = RLColor.White;
			colorMap["black"] = RLColor.Black;
			colorMap["magenta"] = RLColor.Magenta;
			colorMap["green"] = RLColor.LightGreen;
		}
		
		
		public RLColor this[string s]
		{
			get {
				RLColor result;
				if (colorMap.TryGetValue(s, out result)) {
					return result;
				} else {
					return RLColor.Red;
				}
			}
			set {
				// if this isn't static, we're going to want it to be settable
			}
		}
	}
}
