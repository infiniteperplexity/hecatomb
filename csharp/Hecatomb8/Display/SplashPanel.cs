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
    public class SplashPanel : InterfacePanel
    {
        List<ColoredText> CurrentText;
        public SplashPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            Zindex = 1;
            LeftMargin = 2;
            RightMargin = 2;
            TopMargin = 2;
            Active = false;
            List<ColoredText> CurrentText = new List<ColoredText>();
        }

        public override void Draw()
        {
            // eventually want some kind of brief freeze to keep from instantly closing this
            OldGame.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            Vector2 v;
            v = new Vector2(X0, Y0);
            OldGame.Sprites.DrawString(Font, new string('=', 55), v, OldGame.Colors["yellow"]);
            for (var i = 1; i <= 11; i++)
            {
                v = new Vector2(X0, Y0 + CharHeight * i);
                OldGame.Sprites.DrawString(Font, "#", v, OldGame.Colors["yellow"]);
                v = new Vector2(X0 + CharHeight * 30 + 7, Y0 + CharHeight * i);
                OldGame.Sprites.DrawString(Font, "#", v, OldGame.Colors["yellow"]);
            }
            v = new Vector2(X0, Y0 + CharHeight * 12);
            OldGame.Sprites.DrawString(Font, new string('=', 55), v, OldGame.Colors["yellow"]);
            DrawLines(CurrentText);
        }

        public void Splash(List<ColoredText> lines, bool frozen = false, Action callback = null, ColoredText logText = null)
        {
            Active = true;
            Dirty = true;
            if (logText != null)
            {
                OldGame.InfoPanel.PushMessage(logText);
            }
            if (!frozen)
            {
                // this freezes the controls for one second and then sets the callback, right?
                ControlContext.Set(new FrozenControls());
                Thread thread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    var sc = new SplashControls();
                    sc.MyCallback = callback;
                    ControlContext.SetWithoutRedraw(sc);
                });
                thread.Start();
            }
            else
            {
                ControlContext.Set(new FrozenControls());
            }
            CurrentText = lines;
        }

        public void Reset()
        {
            Active = false;
            InterfacePanel.DirtifyUsualPanels();
        }
    }
}
