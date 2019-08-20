using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Hecatomb
{ 
    public class InterfaceElement
    {
        public bool Dirty = true;
        public SpriteFont Font;
        public int Height;
        public int Width;
        public int CharWidth;
        public int CharHeight;
        public int XPad;
        public int YPad;

        public InterfaceElement()
        {
            Dirty = true;
        }
        public void Draw(NewGamePanel panel, int y1)
        {
            //Dirty = true;
            if (Dirty)
            {
                var bg = new Texture2D(panel.Graphics.GraphicsDevice, panel.PixelWidth - 2, panel.PixelHeight - 2);
                Color cbg = Color.Yellow;
                Color[] bgdata = new Color[(panel.PixelWidth - 2) * (panel.PixelHeight - 2)];
                for (int i = 0; i < bgdata.Length; ++i)
                {
                    bgdata[i] = Color.White;
                }
                bg.SetData(bgdata);
                var vbg = new Vector2(panel.X0 + 1, panel.Y0 + 1 + y1);
                panel.Sprites.Draw(bg, vbg, cbg);
                Dirty = false;
            }
        }
    }
}
