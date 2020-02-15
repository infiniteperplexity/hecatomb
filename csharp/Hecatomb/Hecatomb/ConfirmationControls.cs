/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
    /// <summary>
    /// Description of MenuChoiceContext.
    /// </summary>
    /// 

    public class ConfirmationControls : ControlContext
    {
        public string Header;
        public ConfirmationControls(ColoredText header, Action confirmed) : base()
        {
            AlwaysPaused = true;
            MenuSelectable = false;
            Header = header;
            var Commands = Game.Commands;
            KeyMap[Keys.Escape] = Back;
            KeyMap[Keys.Y] = confirmed;
            KeyMap[Keys.N] = Back;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            MenuTop = new List<ColoredText>() {
                "{orange}**Really " + Header + "?**.",
                "{orange}Y) Yes.",
                "{orange}N) No."
            };
            Game.InfoPanel.Dirty = true;
        }

        public override void HandleClick(int x, int y)
        {

        }
        public override void HandleHover(int x, int y)
        {

        }
    }
}
