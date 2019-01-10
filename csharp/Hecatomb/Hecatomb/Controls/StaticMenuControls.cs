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
		public StaticMenuControls(ColoredText header, List<(Keys, ColoredText, Action)> choices) : base()
        {
            MenuTop = new List<ColoredText>() {
                header
            };
            for (int i = 0; i < choices.Count; i++)
            {
                var key = choices[i].Item1;
                ColoredText ct = choices[i].Item2;
                KeyMap[key] = choices[i].Item3;
                string s = key.ToString();
                ct.Text = (s + ") " + ct.Text);
                MenuTop.Add(ct);
            }
        }
	}
}