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

    public class CommandsPanel : InterfacePanel
    {
        public CommandsPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {

            LeftMargin = 2;
            RightMargin = 2;
        }

        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            int total = 0;
            int margin = 4 * CharWidth; 
            for (int i = 0; i < Game.Controls.MenuCommands.Count; i++)
            {
                if (Game.World != null && Game.World.GetState<TutorialHandler>().Visible)
                {
                    var text = Game.World.GetState<TutorialHandler>().Current.MenuCommands[i];
                    var color = (text.Colors.ContainsKey(0)) ? text.Colors[0] : "white";
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
                    total += bump;
                }
                else
                {
                    var command = Game.Controls.MenuCommands[i];
                    var text = command.Item2;
                    var color = "white";
                    if (command.Item1 == Game.Controls.SelectedMenuCommand)
                    {
                        color = "yellow";
                    }
                    else if (command.Item1 == "Tutorial" && Game.World != null && Game.World.GetState<TutorialHandler>().Visible)
                    {
                        color = "cyan";
                    }
                    else if (!Game.Controls.MenuSelectable)
                    {
                        color = "gray";
                    }
                    else if (command.Item1 == "Log" && Game.World.GetState<MessageHandler>().Unread)
                    {
                        color = "orange";
                    }
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
                    total += bump;
                }
            }
        }
    }
}
