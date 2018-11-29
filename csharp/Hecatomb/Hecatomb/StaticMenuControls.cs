/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 3:22 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	public class StaticMenuControls : ControlContext
	{
		public StaticMenuControls(List<Keys> keys, List<ColoredText> names, List<Action> actions): base()
		{
			MenuTop = new List<ColoredText>();
			for (int i=0; i<keys.Count; i++)
			{
				KeyMap[keys[i]] = actions[i];
                // concatenation gets weird here...
				MenuTop.Add(keys[i].ToString()+ ") "+names[i]);
			}
		}
	}
}