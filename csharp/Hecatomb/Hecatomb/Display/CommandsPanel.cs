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
            var c = Game.Controls;
            c.RefreshContent();
            var tutorial = (Game.Time.Frozen || Game.World == null) ? null : Game.World.GetState<TutorialHandler>();

            List<ColoredText> MenuTop = c.MenuTop;
            //List<ColoredText> MenuMiddle = c.MenuMiddle;
            if (!Game.Time.Frozen && tutorial != null && tutorial.Visible)
            {
                if (!tutorial.Current.RequiresDefaultControls || Game.Controls == Game.DefaultControls)
                {
                    MenuTop = tutorial.Current.ControlText;
                    //MenuMiddle = tutorial.Current.InstructionsText;
                }
                else if (Game.Controls == Game.CameraControls)
                {
                    //MenuMiddle = tutorial.OffTutorialCamera;
                }
                else
                {
                    //MenuMiddle = tutorial.OffTutorialText;
                }
            }
            List<ColoredText> text;
            text = MenuTop.ToList();
            //if (MenuMiddle.Count > 0)
            //{
            //    text.Add(" ");
            //}
            //int i0 = text.Count;
            //text = text.Concat(MenuMiddle).ToList();
            //if (c.MenuBottom.Count > 0)
            //{
            //    text.Add(" ");
            //}
            //int i1 = text.Count;
            //text = text.Concat(c.MenuBottom).ToList();
            var topMargin = (Game.World == null) ? 150 : 0;
            var leftMargin = (Game.World == null) ? 150 : 0;
            DrawLines(text, topMargin: topMargin, leftMargin: leftMargin);
        }
    }
}
