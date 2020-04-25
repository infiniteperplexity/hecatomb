using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Hecatomb8
{
    public class InterfacePanel
    {
        public bool Dirty;
        protected SpriteBatch Sprites;
        protected GraphicsDevice Graphics;
        public int X0;
        public int Y0;
        public int CharWidth;
        public int CharHeight;
        public int XPad;
        public int YPad;
        protected Texture2D BG;
        public int Zindex;
        public int PixelWidth;
        public int PixelHeight;
        // text stuff
        public SpriteFont Font;
        public int LeftMargin;
        public int TopMargin;
        public int RightMargin;
        public int BottomMargin;
        public bool Active;
        public List<(string text, Vector2 v, Color color)> DrawableLines;

        public InterfacePanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y)
        {
            Dirty = true;
            Font = c.Load<SpriteFont>("PTMono");
            Graphics = g;
            Sprites = sb;
            X0 = x;
            Y0 = y;
            CharHeight = 16;
            CharWidth = 9;
            XPad = 3;
            YPad = 3;
            PixelHeight = 700;
            PixelWidth = 280;
            BG = new Texture2D(Graphics, CharWidth + XPad, CharHeight + YPad);
            Color[] bgdata = new Color[(CharWidth + XPad) * (CharHeight + YPad)];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            BG.SetData(bgdata);
            DrawableLines = new List<(string, Vector2, Color)>();
            Active = true;
        }

        public virtual void Draw()
        {
            
            foreach (var line in DrawableLines)
            {
                Sprites.DrawString(Font, line.text, line.v, line.color);
            }
        }

        public virtual void Prepare()
        {

        }
        public void PrepareLines(List<ColoredText> lines, int leftMargin = 0, int topMargin = 0)
        {
            if (!Dirty)
            {
                return;
            }
            DrawableLines.Clear();
            Vector2 v;
            // ouput column
            int x = 0;
            // output row
            int y = 0;
            string fg = "white";
            string text = "";
            SortedList<int, string> colors;
            // input row
            for (int i = 0; i < lines.Count; i++)
            {
                text = lines[i].Text;
                colors = lines[i].Colors;
                // advance by one line for every new line of input
                //y++;
                // return to left margin
                x = 0;
                // initialize to white
                fg = "white";
                // input column
                //Debug.WriteLine("calc width: " + )
                for (int j = 0; j < text.Length; j++)
                {
                    if (text.Substring(j, 1) == " ")
                    {
                        for (int k = 1; k < text.Length - j; k++)
                        {
                            if (text.Substring(j + k, 1) != " ")
                            {
                                // I have no idea if this spacing is even right
                                if (x >= (((leftMargin + PixelWidth - CharWidth * LeftMargin - CharWidth * RightMargin) / CharWidth) - 13))
                                {
                                    j += k;
                                    x = 0;
                                    y++;
                                }
                            }
                        }
                    }
                    if (colors.ContainsKey(j))
                    {
                        fg = colors[j];
                    }
                    if ((y + 1) * CharHeight < PixelHeight)
                    {
                        v = new Vector2(leftMargin + X0 + CharWidth * LeftMargin + x * CharWidth, topMargin + TopMargin * CharHeight + Y0 + y * CharHeight);
                        //Sprites.DrawString(Font, text.Substring(j, 1), v, InterfaceState.Colors![fg]);
                        DrawableLines.Add((text.Substring(j, 1), v, InterfaceState.Colors![fg]));
                    }
                    x += 1;
                }
                y++;
            }
            Dirty = false;
        }
    }
}
