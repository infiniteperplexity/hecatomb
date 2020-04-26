using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Threading;

namespace Hecatomb8
{
    public class PopupPanel : InterfacePanel
    {
        public PopupPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y, int w, int h) : base(g, sb, c, x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
            TopMargin = 2;
            Active = false;
        }

        public override void Prepare()
        {
            if (Active)
            {
                var splash = (SplashControls)InterfaceState.Controls;
                PrepareLines(splash.SplashText);
            }
        }

        public override void PrepareLines(List<ColoredText> lines, int leftMargin = 0, int topMargin = 0)
        {
            if (!Dirty)
            {
                return;
            }
            base.PrepareLines(lines);
            Vector2 v;
            v = new Vector2(X0, Y0);
            DrawableLines.Add((new string('=', 55), v, InterfaceState.Colors!["yellow"]));
            for (var i = 1; i <= 11; i++)
            {
                v = new Vector2(X0, Y0 + CharHeight * i);
                DrawableLines.Add(("#", v, InterfaceState.Colors!["yellow"]));
                v = new Vector2(X0 + CharHeight * 30 + 7, Y0 + CharHeight * i);
                DrawableLines.Add(("#", v, InterfaceState.Colors!["yellow"]));
            }
            v = new Vector2(X0, Y0 + CharHeight * 12);
            DrawableLines.Add((new string('=', 55), v, InterfaceState.Colors!["yellow"]));
            Dirty = false;
        }

    }
}
