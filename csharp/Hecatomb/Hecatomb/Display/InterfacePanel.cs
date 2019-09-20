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
using System.Threading;

namespace Hecatomb
{

    public class InterfacePanel
    {
        public static List<List<InterfacePanel>> Panels = new List<List<InterfacePanel>>();
        public bool Dirty;
        public bool Active;
        // basic stuff
        protected Texture2D BG;
        public int X0;
        public int Y0;
        public int Zindex;
        public int XPad;
        public int YPad;
        public int PixelWidth;
        public int PixelHeight;
        // text stuff
        public SpriteFont Font;
        public int CharWidth;
        public int CharHeight;
        public int LeftMargin;
        public int TopMargin;
        public int RightMargin;
        public int BottomMargin;
        // should we have a textbuffer by default?


        public InterfacePanel(int x, int y, int w, int h)
        {
            Active = true;
            Dirty = true;
            X0 = x;
            Y0 = y;
            Zindex = 0;
            PixelWidth = w;
            PixelHeight = h;
            Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
            CharHeight = 16;
            CharWidth = 9;
            XPad = 0;
            YPad = 0;
            RightMargin = 1;
            LeftMargin = 1;
            BG = new Texture2D(Game.Graphics.GraphicsDevice, w, h);
            Color[] bgdata = new Color[w * h];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            BG.SetData(bgdata);
        }

        public static void AddPanel(InterfacePanel ip)
        {
            int z = ip.Zindex;           
            while (Panels.Count < z + 1)
            {
                Panels.Add(new List<InterfacePanel>());
            }
            Panels[z].Add(ip);
        }

        public static void DrawPanels()
        {
            foreach (var list in Panels)
            {
                foreach (var panel in list)
                {
                    if (panel.Dirty && panel.Active)
                    {
                        panel.Draw();
                        panel.Dirty = false;
                    }
                }
            }
        }

        // draw a blank panel
        public virtual void Draw()
        {
            var bg = new Texture2D(Game.Graphics.GraphicsDevice, PixelWidth - 2, PixelHeight - 2);
            Color cbg = Color.DarkGray;
            Color[] bgdata = new Color[(PixelWidth - 2) * (PixelHeight - 2)];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            bg.SetData(bgdata);
            var vbg = new Vector2(X0 + 1, Y0 + 1);
            Game.Sprites.Draw(bg, vbg, cbg);
        }


        public void DrawLines(List<ColoredText> lines, int leftMargin = 0, int topMargin = 0)
        {
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
                y++;
                // return to left margin
                x = 0;
                // initialize to white
                fg = "white";
                // input column
                for (int j = 0; j < text.Length; j++)
                {
                    if (text.Substring(j, 1) == " ")
                    {
                        for (int k = 1; k < text.Length - j; k++)
                        {
                            if (text.Substring(j + k, 1) != " ")
                            {
                                // I have no idea if this spacing is even right
                                if (x >= (((leftMargin + PixelWidth - CharWidth * LeftMargin - CharWidth * RightMargin) / CharWidth) - 0 /* - 8 */))
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
                    v = new Vector2(leftMargin + X0 + CharWidth * LeftMargin + x * CharWidth, topMargin + TopMargin + Y0 + y * CharHeight);
                    Game.Sprites.DrawString(Font, text.Substring(j, 1), v, Game.Colors[fg]);
                    x += 1;
                }
            }
        }


            public static InterfacePanel GetPanel(int x, int y)
        {
            for (int i = Panels.Count - 1; i >= 0; i--)
            {
                var list = Panels[i];
                foreach (var panel in list)
                {
                    if (panel.Active && panel.X0 <= x && panel.Y0 <= y && panel.X0 + panel.PixelWidth > x && panel.Y0 + panel.PixelHeight > y)
                    {
                        return panel;
                    }
                }
            }
            return null;
        }

        public static void DirtifySidePanels()
        {
            if (Panels.Count > 0)
            {
                foreach (var panel in Panels[0])
                {
                    if (!(panel is MainPanel))
                    {
                        panel.Dirty = true;
                    }
                }
            }
        }

        public static void DirtifyUsualPanels()
        {
            if (Panels.Count > 0)
            {
                foreach (var panel in Panels[0])
                {
                    panel.Dirty = true;
                }
            }
        }

        public static void DirtifyMainPanel()
        {
            if (Panels.Count > 0)
            {
                foreach (var panel in Panels[0])
                {
                    if (panel is MainPanel)
                    {
                        panel.Dirty = true;
                    }
                }
            }
        }
    }

    

    

    

    

    
}