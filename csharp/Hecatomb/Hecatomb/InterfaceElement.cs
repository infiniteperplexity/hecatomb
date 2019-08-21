﻿using System;
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
        public int LeftMargin;
        public int TopMargin;
        public int RightMargin;
        public int BottomMargin;
        public int XPad;
        public int YPad;
        public Texture2D BG;
        public NewGamePanel Panel;

        public InterfaceElement()
        {
            Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
            CharHeight = 16;
            CharWidth = 9;
            Dirty = true;
        }
        public virtual void Draw(int y1)
        {
            //Dirty = true;
            if (Dirty)
            {
                var bg = new Texture2D(Game.Graphics.GraphicsDevice, Panel.PixelWidth - 2, Panel.PixelHeight - 2);
                Color cbg = Color.Yellow;
                Color[] bgdata = new Color[(Panel.PixelWidth - 2) * (Panel.PixelHeight - 2)];
                for (int i = 0; i < bgdata.Length; ++i)
                {
                    bgdata[i] = Color.White;
                }
                bg.SetData(bgdata);
                var vbg = new Vector2(Panel.X0 + 1, Panel.Y0 + 1 + y1);
                Game.Sprites.Draw(bg, vbg, cbg);
                Dirty = false;
            }
        }

        public void DrawLines(List<ColoredText> lines)
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
                                if (x >= (((Panel.PixelWidth - LeftMargin - RightMargin)/ CharWidth) - 8))
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
                    v = new Vector2(LeftMargin + Panel.X0 + x * CharWidth, TopMargin + Panel.Y0  + y * CharHeight);
                    Panel.Sprites.DrawString(Font, text.Substring(j, 1), v, Game.Colors[fg]);
                    x += 1;
                }
            }
        }
    }

    public class MainViewElement : InterfaceElement
    {
        List<SpriteFont> Fonts;
        Dictionary<char, ValueTuple<Vector2, SpriteFont>> fontCache;
        
        public MainViewElement()
        {
            CharWidth = 18;
            CharHeight = 18;
            XPad = 3;
            YPad = 3;
            BG = new Texture2D(Game.Graphics.GraphicsDevice, CharWidth + XPad, CharHeight + YPad);
            Color[] bgdata = new Color[(CharWidth + XPad) * (CharHeight + YPad)];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            BG.SetData(bgdata);
            Fonts = new List<SpriteFont>();
            string[] fonts = new string[] { "NotoSans", "NotoSansSymbol", "NotoSansSymbol2", "Cambria" };
            foreach (string s in fonts)
            {
                SpriteFont font = Game.MyContentManager.Load<SpriteFont>(s);
                Fonts.Add(font);
            }
            fontCache = new Dictionary<char, ValueTuple<Vector2, SpriteFont>>();
        }
        public override void Draw(int y1)
        {

            if (Game.World == null || !World.WorldSafeToDraw)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        Game.Sprites.Draw(BG, new Vector2(Panel.X0 + XPad + (1 + i) * (CharWidth + XPad), Panel.Y0 + YPad + (1 + j) * (CharHeight + YPad)), Color.Black);
                    }
                }
                return;
            }
            Camera Camera = Game.Camera;
            var grid = Game.World.Terrains;
            int z = Camera.Z;
            //ValueTuple<char, string, string> glyph;
            for (int i = 0; i < Camera.Width; i++)
            {
                for (int j = 0; j < Camera.Height; j++)
                {
                    int x = i + Camera.XOffset;
                    int y = j + Camera.YOffset;
                    var (sym, fg, bg) = Tiles.GetGlyph(x, y, z);
                    DrawGlyph(i, j, sym, fg, bg);
                }
            }
        }

        public void DrawGlyph(int i, int j, char c, string fg, string bg)
        {
            string s = c.ToString();
            var (measure, font) = resolveFont(c);
            int xOffset = 11 - (int)measure.X / 2;
            int yOffset = (int)measure.Y;
            Color cfg = (fg == null) ? Color.Red : Game.Colors[fg];
            Color cbg = (bg == null) ? Color.Red : Game.Colors[bg];
            var vbg = new Vector2(Panel.X0 + XPad + i * (CharWidth + XPad), Panel.Y0 + YPad + j * (CharHeight + YPad));
            var vfg = new Vector2(Panel.X0 + xOffset + XPad + i * (CharWidth + XPad), Panel.Y0 + yOffset + YPad + j * (CharHeight + YPad));
            //var vbg = new Vector2(Padding + (i) * (Size + Padding), Padding + (j) * (Size + Padding));
            //var vfg = new Vector2(xOffset + Padding + (i) * (Size + Padding), yOffset + Padding + (j) * (Size + Padding));
            Game.Sprites.Draw(BG, vbg, cbg);
            if (c == '\u02C7')
            {
                Panel.Sprites.DrawString(Fonts[0], "^", vfg + (new Vector2(-2, 0)), cfg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
            }
            else
            {
                Panel.Sprites.DrawString(font, s, vfg, cfg);
            }
        }

        protected (Vector2, SpriteFont) resolveFont(char c)
        {
            if (c == default(char))
            {
                return (new Vector2(0, 0), Fonts[0]);
            }
            if (fontCache.ContainsKey(c))
            {
                return fontCache[c];
            }
            string s = c.ToString();
            foreach (SpriteFont f in Fonts)
            {
                if (f.GetGlyphs().ContainsKey(c))
                {

                    Vector2 v = f.MeasureString(c.ToString());
                    if (Fonts[0].GetGlyphs().ContainsKey(c))
                    {
                        fontCache[c] = (new Vector2(v.X, -7), f);
                    }
                    else if (Fonts[1].GetGlyphs().ContainsKey(c))
                    {

                        fontCache[c] = (new Vector2(v.X, -17), f);
                        if (c == '\u271D')
                        {
                            fontCache[c] = (new Vector2(v.X, -16), f);
                        }
                    }
                    else if (Fonts[2].GetGlyphs().ContainsKey(c))
                    {

                        fontCache[c] = (new Vector2(v.X + 1, -7), f);
                    }
                    else if (Fonts[3].GetGlyphs().ContainsKey(c))
                    {
                        fontCache[c] = (new Vector2(v.X + 1, -5), f);
                    }
                    if (c == '\u2717')
                    {
                        fontCache[c] = (new Vector2(v.X, -9), f);
                    }
                    return fontCache[c];
                }
            }
            throw new InvalidOperationException(String.Format("No font found for symbol {0}", c));
        }
    }

    public class InstructionsElement : InterfaceElement
    {
        public InstructionsElement(int width, int height)
        {
            LeftMargin = 2*CharWidth;
            RightMargin = 2*CharWidth;
            BG = new Texture2D(Game.Graphics.GraphicsDevice, width, height);
            Color[] bgdata = new Color[width * height];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            BG.SetData(bgdata);
        }



        public override void Draw(int y1)
        {
            Game.Sprites.Draw(BG, new Vector2(Panel.X0, Panel.Y0), Color.Black);
            var c = Game.Controls;
            c.RefreshContent();
            var tutorial = (Game.Time.Frozen || Game.World == null) ? null : Game.World.GetState<TutorialHandler>();

            List<ColoredText> MenuTop = c.MenuTop;
            List<ColoredText> MenuMiddle = c.MenuMiddle;
            if (!Game.Time.Frozen && tutorial != null && tutorial.Visible)
            {
                if (!tutorial.Current.RequiresDefaultControls || Game.Controls == Game.DefaultControls)
                {
                    MenuTop = tutorial.Current.ControlText;
                    MenuMiddle = tutorial.Current.InstructionsText;
                }
                else if (Game.Controls == Game.CameraControls)
                {
                    MenuMiddle = tutorial.OffTutorialCamera;
                }
                else
                {
                    MenuMiddle = tutorial.OffTutorialText;
                }
            }
            List<ColoredText> text;
            text = MenuTop.ToList();
            if (MenuMiddle.Count > 0)
            {
                text.Add(" ");
            }
            int i0 = text.Count;
            text = text.Concat(MenuMiddle).ToList();
            if (c.MenuBottom.Count > 0)
            {
                text.Add(" ");
            }
            int i1 = text.Count;
            text = text.Concat(c.MenuBottom).ToList();
            DrawLines(text);
        }
    }
}
