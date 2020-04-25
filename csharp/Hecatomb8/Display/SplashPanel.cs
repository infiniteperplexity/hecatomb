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
    public class SplashPanel : InterfacePanel
    {
        List<ColoredText> CurrentText;
        public SplashPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y) : base(g, sb, c, x, y)
        {
            Zindex = 1;
            LeftMargin = 2;
            RightMargin = 2;
            TopMargin = 2;
            Active = false;
            List<ColoredText> CurrentText = new List<ColoredText>();
        }

        //public override void PrepareLines()
        //{
        //    for (var i = 1; i <= 11; i++)
        //    {

        //    }
        //Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
        //Vector2 v;
        //v = new Vector2(X0, Y0);
        //Game.Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
        //for (var i = 1; i <= 11; i++)
        //{
        //    v = new Vector2(X0, Y0 + CharHeight * i);
        //    Game.Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
        //    v = new Vector2(X0 + CharHeight * 30 + 7, Y0 + CharHeight * i);
        //    Game.Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
        //}
        //v = new Vector2(X0, Y0 + CharHeight * 12);
        //Game.Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
        //DrawLines(CurrentText);
        //}

        public void Splash(List<ColoredText> lines, bool frozen = false, Action? callback = null, ColoredText? logText = null)
        {
        }
    }
}
