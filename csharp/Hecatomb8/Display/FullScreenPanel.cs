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
    public class FullScreenPanel : InterfacePanel
    {
        public FullScreenPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y, int w, int h) : base(g, sb, c, x, y, w, h)
        {
            LeftMargin = 3;
            TopMargin = 3;
            Active = false;
        }

        public override void Prepare()
        {
            if (Active && InterfaceState.Controls is SplashControls)
            { 
                var splash = (SplashControls)InterfaceState.Controls;
                PrepareLines(splash.SplashText);
            }
        }
    }
}
