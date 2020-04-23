using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{

    public class StaticMenuControls : ControlContext
    {
        public StaticMenuControls(ColoredText header, List<(Keys, ColoredText, Action)> choices) : base()
        {
            //AlwaysPaused = true;
            //MenuSelectable = false;
            MenuTop = new List<ColoredText>();
            if (header != " ")
            {
                MenuTop.Add(header);
                MenuTop.Add(" ");
            }
            for (int i = 0; i < choices.Count; i++)
            {
                // allow for blank lines by setting the third item equal to null
                if (choices[i].Item3 == null)
                {
                    MenuTop.Add(" ");
                }
                else
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
}