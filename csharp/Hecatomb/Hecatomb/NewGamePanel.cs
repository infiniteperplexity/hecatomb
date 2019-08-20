/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/5/2018
 * Time: 9:57 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{

    public class NewGamePanel
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch Sprites;
        public Texture2D BG;
        public int X0;
        public int Y0;
        public int PixelWidth;
        public int PixelHeight;
        public List<InterfaceElement> Elements;


        public NewGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites, int x0, int y0)
        {
            Graphics = graphics;
            Sprites = sprites;
            X0 = x0;
            Y0 = y0;
            Elements = new List<InterfaceElement>();
        }

        public virtual void DrawElements() {
            int y1 = 0;
            foreach (var el in Elements)
            {
                el.Draw(this, y1);
                y1 += el.Height;
            }

        }
    }
}