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
    public class FullScreenPanel : InterfacePanel
    {
        List<ColoredText> CurrentText;
        public FullScreenPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            Zindex = 1;
            LeftMargin = 3;
            TopMargin = 3;
            Active = false;
        }

        public override void Draw()
        {
            // eventually want some kind of brief freeze to keep from instantly closing this
            OldGame.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            DrawLines(CurrentText);
        }
        public void Splash(List<ColoredText> lines, bool frozen = false)
        {
            Active = true;
            Dirty = true;
            if (!frozen)
            {
                ControlContext.Set(new SplashControls());
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
            Debug.WriteLine("flag 2");
            InterfacePanel.DirtifyUsualPanels();
        }
    }
}
