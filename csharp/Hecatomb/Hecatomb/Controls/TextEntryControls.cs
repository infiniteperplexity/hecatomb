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

    public class TextEntryControls : ControlContext
    {
        public string Header;
        public string CurrentText;
        public int MaxTextLength;
        public static List<Keys> Alphabet = new List<Keys> {
            Keys.A,
            Keys.B,
            Keys.C,
            Keys.D,
            Keys.E,
            Keys.F,
            Keys.G,
            Keys.H,
            Keys.I,
            Keys.J,
            Keys.K,
            Keys.L,
            Keys.M,
            Keys.N,
            Keys.O,
            Keys.P,
            Keys.Q,
            Keys.R,
            Keys.S,
            Keys.T,
            Keys.U,
            Keys.V,
            Keys.W,
            Keys.X,
            Keys.Y,
            Keys.Z,
        };

        public static string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public TextEntryControls(ColoredText header, Action<string> submit) : base()
        {
            MenuSelectable = false;
            CurrentText = Game.GameName;
            Header = header;
            MaxTextLength = 25;
            Throttle = 250;
            var Commands = Game.Commands;
            KeyMap[Keys.Escape] = Reset;
            KeyMap[Keys.Enter] = ()=> { submit(CurrentText); };
            KeyMap[Keys.Back] = Backspace;
            foreach (Keys key in Alphabet)
            {
                KeyMap[key] = LetterClosure(key);
            }
            RefreshContent();
        }

        public Action LetterClosure(Keys key)
        {
            // this guy doesn't throttle very well
            return () => {
                if (CurrentText.Length>=MaxTextLength)
                {
                    return;
                }
                string s = Enum.GetName(typeof(Keys), key);
                // check if shift is down
                if (ControlContext.ShiftDown)
                {
                    CurrentText += s;
                }
                else
                {
                    CurrentText += s.ToLower();
                }
                RefreshContent();
                // do I need to make the panel dirty?
            };
        }

        public void Backspace()
        {
            if (CurrentText.Length==0)
            {
                return;
            }
            CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
            RefreshContent();
        }

        public override void RefreshContent()
        {
            MenuTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                " ",
                ("{yellow}"+Header),
                (CurrentText+"_"),
                "{cyan}Press Enter when finished."
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
