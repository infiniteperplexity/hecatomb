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

namespace Hecatomb8
{
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
            Keys.D0,
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
            Keys.NumPad0,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.OemMinus
        };

        public static string alphabet = "abcdefghijklmnopqrstuvwxyz01234567890123456789_";

        public TextEntryControls(ColoredText header, Action<string> submit) : base()
        {
            AllowsUnpause = false;
            //MenuSelectable = false;
            CurrentText = GameManager.GameName;
            Header = header;
            MaxTextLength = 25;
            Throttle = 250;
            var Commands = InterfaceState.Commands;
            KeyMap[Keys.Escape] = InterfaceState.ResetControls;
            KeyMap[Keys.Enter] = () => { submit(CurrentText); };
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
                if (CurrentText.Length >= MaxTextLength)
                {
                    return;
                }
                //string s = Enum.GetName(typeof(Keys), key);
                char s = alphabet[Alphabet.IndexOf(key)];
                if (Char.IsDigit(s) && CurrentText.Length == 0)
                {
                    return;
                }
                // check if shift is down
                if (ControlContext.ShiftDown)
                {
                    CurrentText += s.ToString().ToUpper();
                }
                else
                {
                    CurrentText += s;
                }
                RefreshContent();
                // do I need to make the panel dirty?
            };
        }

        public void Backspace()
        {
            if (CurrentText.Length == 0)
            {
                return;
            }
            CurrentText = CurrentText.Substring(0, CurrentText.Length - 1);
            RefreshContent();
        }

        public override void RefreshContent()
        {
            InfoTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                " ",
                ("{yellow}"+Header),
                (CurrentText+"_"),
                "{cyan}Press Enter when finished."
            };
        }

        public override void HandleClick(int x, int y)
        {

        }
        public override void HandleHover(int x, int y)
        {

        }
    }
}
