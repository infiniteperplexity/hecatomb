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
using System.Linq;

namespace Hecatomb
{
    /// <summary>
    /// Description of MenuChoiceContext.
    /// </summary>
    /// 

    public class MessageLogControls : ControlContext
    {
        public int SelectedMessage;

        public MessageLogControls() : base()
        {
            MenuSelectable = false;
            RefreshContent();
            Game.MenuPanel.Dirty = true;
        }

        public override void RefreshContent()
        {
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] = Reset;
            KeyMap[Keys.PageUp] = ScrollUp;
            KeyMap[Keys.PageDown] = ScrollDown;
            int MaxVisible = Math.Min(Game.World.GetState<MessageHandler>().MessageHistory.Count, 4);
            var controls = new List<ColoredText>()
            {
                "{orange}**Esc) Back**.",
                " ",
                "{yellow}Message Log:",
                "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
            };
            var list = Game.World.GetState<MessageHandler>().MessageHistory.GetRange(SelectedMessage, MaxVisible).ToList();
            if (list.Count > 1 && list[1].Colors.Count == 0)
            {
                list[0] = new ColoredText(list[1].Text, "cyan");
            }
            if (list.Count==0)
            {
                list.Add("{cyan}Welcome to Hecatomb!");
            }
            list = controls.Concat(list).ToList();
            list.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            list.Add("Scroll: Page/Fn Up/Down");
            MenuTop = list.ToList();
        }

        // huh...so...the "ShowScroll" thing...do we still want that?
        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            Game.World.GetState<MessageHandler>().MessageHistory.Insert(0, ct);
            while (Game.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
            {
                Game.World.GetState<MessageHandler>().MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
            RefreshContent();
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
            RefreshContent();
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < Game.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
            RefreshContent();
        }


    }
}
