using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace Hecatomb
{
    /* This will want...
     * 1) Camera / avatar mode.
     * 2) Spells.
     * 3) Jobs.
     * 4) Help / Key shortcuts
     * 5) Toggle tutorial.
     * 6) Message log.
     * 7) Overview.
     * 8) Hover.
     * 9) Pause and speed.
     * 10) Achievements.
     * 11) Tech tree.
     * 12) Structures.
     * 13) Minions.
     * 14) System view.
     */

    class CommandMenu
    {
        public int Width;
        public string Title;
        public Func<ColoredText> GetText;
        public CommandMenu(string title, Keys k)
        {
            Title = title;
            Width = 120;
        }
    }
    public class CommandsPanel : InterfacePanel
    {
        List<CommandMenu> CommandMenus;
        int ActiveMenu;

        public CommandsPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {

            LeftMargin = 2;
            RightMargin = 2;
            CommandMenus = new List<CommandMenu>();
            CommandMenu menu;
            ActiveMenu = 1;
            menu = new CommandMenu("Esc: Game", Keys.Escape);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("0: Tutorial", Keys.NumPad0);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("1: Messages", Keys.NumPad1);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("2: Overview", Keys.NumPad2);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("3: Spells", Keys.NumPad3);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("4: Jobs", Keys.NumPad4);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("5: Achievements", Keys.NumPad5);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("6: Research", Keys.NumPad6);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("?: Help", Keys.OemQuestion);
            menu.GetText = () =>
            {
                return "X: Command";
            };
            CommandMenus.Add(menu);
        }

        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            for (int i = 0; i < CommandMenus.Count; i++)
            {
                var menu = CommandMenus[i];
                var text = menu.Title;
                var color = "white";
                if (i == ActiveMenu)
                {
                    color = "orange";
                }
                var v = new Vector2(X0 + LeftMargin + i * menu.Width, Y0 + TopMargin);
                Debug.WriteLine(text);
                Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
            }
        }
    }
}
