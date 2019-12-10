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
        public MessageLogControls() : base()
        {
            MenuSelectable = false;
            RefreshContent();
            Game.MenuPanel.Dirty = true;
        }

        public void ExitLogMode()
        {
            LogMode = false;
            Reset();
        }
        public override void RefreshContent()
        {
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] = ExitLogMode;
            KeyMap[Keys.L] = ExitLogMode;   
            KeyMap[Keys.PageUp] = Commands.ScrollUpCommand;
            KeyMap[Keys.PageDown] = Commands.ScrollDownCommand;
            KeyMap[Keys.Z] = Commands.ChooseSpell;
            KeyMap[Keys.J] = Commands.ChooseTask;
            int MaxVisible = Math.Min(Game.World.GetState<MessageHandler>().MessageHistory.Count, 4);
            var controls = new List<ColoredText>()
            {
                "{orange}**Esc) Back**.",
                " ",
                "{yellow}Message Log:",
                "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
            };
            Debug.WriteLine($"SelectedMessage is {Game.InfoPanel.SelectedMessage} and MaxVisible is {MaxVisible}");
            var list = Game.World.GetState<MessageHandler>().MessageHistory.GetRange(Game.InfoPanel.SelectedMessage, MaxVisible).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new ColoredText(list[i]);
            }
            if (list.Count > 1 && list[1].Colors.Count == 0)
            {
                // this used to reference list[1] on the right hand side, I assume that was just a bug
                list[0] = new ColoredText(list[0].Text, "cyan");
            }
            if (list.Count==0)
            {
                list.Add("{cyan}Welcome to Hecatomb!");
            }
            foreach (var txt in list)
            {
                txt.Text = "- " + txt.Text;
            }
            list = controls.Concat(list).ToList();
            list.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            list.Add("Scroll: Page/Fn Up/Down");
            MenuTop = list.ToList();
        }


        public override bool IsMenuSelectable(string s)
        {
            if (s == "Spells" || s == "Jobs")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
