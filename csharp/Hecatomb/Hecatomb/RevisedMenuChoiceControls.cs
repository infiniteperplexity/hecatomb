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
    //public interface IMenuListable
    //{
    //    void ChooseFromMenu();
    //    ColoredText ListOnMenu();
    //}

    //public interface IChoiceMenu
    //{
    //    [JsonIgnore]
    //    string MenuHeader { get; set; }
    //    [JsonIgnore]
    //    List<IMenuListable> MenuChoices { get; set; }
    //}

    public class RevisedMenuChoiceControls : ControlContext
    {
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
            Keys.Z
        };

        public static string alphabet = "abcdefghijklmnopqrstuvwxyz";
        public List<string> Choices;

        public RevisedMenuChoiceControls(IChoiceMenu menu) : base()
        {
            // it currently passes the header line, which could be done in yellow, and it could be done as multiple lines or just one
            // maybe you could pass empty choices?
            // additional choices?
            if (menu is Structure)
            {
                Structure s = (Structure)menu;
                s.HighlightSquares();
                Game.MainPanel.Dirty = true;
            }
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] =
                () =>
                {
                    if (menu is Structure)
                    {
                        Structure s = (Structure)menu;
                        s.Unhighlight();
                    }
                    Reset();
                };
            MenuTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                ("{yellow}"+menu.MenuHeader)
            };
            var choices = menu.MenuChoices;
            // not the real thing to do...
            for (int i = 0; i < choices.Count; i++)
            {
                KeyMap[Alphabet[i]] = choices[i].ChooseFromMenu;
                ColoredText ct = choices[i].ListOnMenu();
                ct.Text = (alphabet[i] + ") " + ct.Text);
                MenuTop.Add(ct);
            }
        }


    }
}
