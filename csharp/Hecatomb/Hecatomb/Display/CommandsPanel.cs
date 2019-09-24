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

    public class CommandMenu
    {
        public string Title;
        public Func<List<ColoredText>> GetText;
        public CommandMenu(string title, Keys k)
        {
            Title = title;
        }
    }
    public class CommandsPanel : InterfacePanel
    {
        public List<CommandMenu> CommandMenus;
        public int ActiveMenu;

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
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("?: Commands", Keys.NumPad0);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("Z: Spells", Keys.NumPad1);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("J: Jobs", Keys.NumPad2);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("L: Message Log", Keys.NumPad3);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("R: Research", Keys.NumPad4);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
            menu = new CommandMenu("V: Achievements", Keys.NumPad5);
            menu.GetText = () =>
            {
                return new List<ColoredText>();
            };
            CommandMenus.Add(menu);
        }

        public CommandMenu GetCommand(string title)
        {
            foreach (var menu in CommandMenus)
            {
                if (menu.Title == title)
                {
                    return menu;
                }
            }
            return null;
        }

        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            int total = 0;
            int margin = 4 * CharWidth; 
            for (int i = 0; i < CommandMenus.Count; i++)
            {
                var menu = CommandMenus[i];
                var text = menu.Title;
                var color = "white";
                if (menu == Game.Controls.LinkedCommand)
                {
                    color = "orange";
                }
                int adjust = (i == 0) ? -2 * CharWidth : 0;
                var v = new Vector2(total + margin + adjust, Y0 + TopMargin);
                var bump = margin + adjust + text.Length * CharWidth + margin;
                //var v = new Vector2(justify + X0 + LeftMargin + i * menu.Width, Y0 + TopMargin);
                Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
                total += bump;
            }
        }
    }
}
