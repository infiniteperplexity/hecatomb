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
		public StaticMenuControls(List<Keys> keys, List<string> names, List<Action> actions, TextColors colors=null): base()
		{
			TopColors = colors ?? TextColors.NoColors;
			MenuTop = new List<string>();
			for (int i=0; i<keys.Count; i++)
			{
				KeyMap[keys[i]] = actions[i];
				MenuTop.Add(keys[i].ToString()+ ") "+names[i]);
			}
		}
	}
}