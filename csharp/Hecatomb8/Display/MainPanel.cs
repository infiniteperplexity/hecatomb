﻿using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Hecatomb8
{
    public class MainPanel : InterfacePanel
    {
        List<SpriteFont> Fonts;
        Dictionary<char, (Vector2, SpriteFont)> fontCache = new Dictionary<char, (Vector2, SpriteFont)>();
        DrawableGlyph?[,] NextGlyphs;
        public string[] IntroLines;
        protected Texture2D CharBG;

        public MainPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, Camera cam, int x, int y, int w, int h) : base(g, sb, c, x, y, w, h)
        {
            CharWidth = 18;
            CharHeight = 18;
            Fonts = new List<SpriteFont>();
            string[] fonts = new string[] { "NotoSans", "NotoSansSymbol", "NotoSansSymbol2", "Cambria" };
            foreach (string s in fonts)
            {
                SpriteFont font = c.Load<SpriteFont>(s);
                Fonts.Add(font);
            }
            fontCache = new Dictionary<char, (Vector2, SpriteFont)>();
            CharBG = new Texture2D(Graphics, CharWidth + XPad, CharHeight + YPad);
            Color[] bgdata = new Color[(CharWidth + XPad) * (CharHeight + YPad)];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            CharBG.SetData(bgdata);
            NextGlyphs = new DrawableGlyph?[cam.Width, cam.Height];
            IntroLines = System.IO.File.ReadAllLines(@"Content/ASCII_icon.txt");       
        }



        // in a better world, this would be a Record Type
        class DrawableGlyph
        {
            public Vector2 fgv;
            public Vector2 bgv;
            public SpriteFont spriteFont;
            public string symbol;
            public Color fg;
            public Color bg;
            public DrawableGlyph(Vector2 fv, Vector2 bv, SpriteFont sf, string s, Color f, Color b)
            {
                fgv = fv;
                bgv = bv;
                spriteFont = sf;
                symbol = s;
                fg = f;
                bg = b;
            }
        }

        public override void Prepare()
        {
            PrepareGlyphs();
        }

        // i and j refer to camera positions, not world coordinates
        public void PrepareGlyph(int i, int j, int z)
        {
            var camera = InterfaceState.Camera!;
            if (GameState.World is null)
            {
                if (j >= IntroLines.Length || i * 2 >= IntroLines[0].Length)
                {
                    return;
                }
                char sym = IntroLines[j][i * 2];
                string fg = "white";
                string bg = "black";
                var str = sym.ToString();
                var (measure, font) = resolveFont(sym);
                int xOffset = 11 - (int)measure.X / 2;
                int yOffset = (int)measure.Y;
                var vbg = new Vector2(X0 + XPad + i * (CharWidth + XPad), Y0 + YPad + j * (CharHeight + YPad));
                var vfg = new Vector2(X0 + xOffset + XPad + i * (CharWidth + XPad), Y0 + yOffset + YPad + j * (CharHeight + YPad));
                if (sym == ' ')
                {
                    return;
                }
                else if (sym == 'z')
                {
                    fg = "lime green";
                }
                else if (sym == '@')
                {
                    fg = "magenta";
                }
                NextGlyphs[i, j] = new DrawableGlyph(vfg, vbg, font, str, InterfaceState.Colors![fg], InterfaceState.Colors![bg]);
            }
            // otherwise, display tiles
            else
            {
                int x = i + camera.XOffset;
                int y = j + camera.YOffset;
                
                var (sym, fg, bg) = Tiles.GetGlyphWithBoundsChecked(x, y, z);
                var str = sym.ToString();
                var (measure, font) = resolveFont(sym);
                int xOffset = 11 - (int)measure.X / 2;
                int yOffset = (int)measure.Y;
                var vbg = new Vector2(X0 + XPad + i * (CharWidth + XPad), Y0 + YPad + j * (CharHeight + YPad));
                var vfg = new Vector2(X0 + xOffset + XPad + i * (CharWidth + XPad), Y0 + yOffset + YPad + j * (CharHeight + YPad));
                NextGlyphs[i, j] = new DrawableGlyph(vfg, vbg, font, str, InterfaceState.Colors![fg], InterfaceState.Colors![bg]);
            }
        }

        public void ClearGlyphs()
        {
            var camera = InterfaceState.Camera!;
            NextGlyphs = new DrawableGlyph?[camera.Width, camera.Height];
        }
        public void PrepareGlyphs()
        {
            var camera = InterfaceState.Camera!;    
            if (Dirty)
            {
                for (int i = 0; i < camera.Width; i++)
                {
                    for (int j = 0; j < camera.Height; j++)
                    {
                        PrepareGlyph(i, j, camera.Z);
                    }
                }
                Dirty = false;
                InterfaceState.OldDirtyTiles.Clear();
                InterfaceState.NextDirtyTiles.Clear();
            }
            else if (InterfaceState.NextDirtyTiles.Count > 0)
            {
                InterfaceState.OldDirtyTiles.UnionWith(InterfaceState.NextDirtyTiles);
                foreach (Coord c in InterfaceState.OldDirtyTiles)
                {
                    int x = c.X - camera.XOffset;
                    int y = c.Y - camera.YOffset;
                    if (x >= 0 && x < camera.Width && y >= 0 && y < camera.Height && c.Z == camera.Z)
                    {
                        PrepareGlyph(x, y, camera.Z);
                    }
                }
                var swap = InterfaceState.OldDirtyTiles;
                InterfaceState.OldDirtyTiles = InterfaceState.NextDirtyTiles;
                InterfaceState.NextDirtyTiles = swap;
                InterfaceState.NextDirtyTiles.Clear();
            }
        }

        public override void Draw()
        {
            //Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            var camera = InterfaceState.Camera!;
            // draw tile backgrounds
            for (int i = 0; i < camera.Width; i++)
            {
                for (int j = 0; j < camera.Height; j++)
                {
                    var glyph = NextGlyphs[i, j];
                    if (glyph != null)
                    {
                        Sprites.Draw(CharBG, glyph.bgv, glyph.bg);
                    }
                }
            }
            // draw tile foregrounds
            for (int i = 0; i < camera.Width; i++)
            {
                for (int j = 0; j < camera.Height; j++)
                {
                    var glyph = NextGlyphs[i, j];
                    if (glyph != null)
                    {
                        if (glyph.symbol == "\u02C7")
                        {
                            Sprites.DrawString(Fonts[0], "^", glyph.fgv + (new Vector2(-2, 0)), glyph.fg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                        }
                        else if (glyph.symbol == "\u2235")
                        {
                            Sprites.DrawString(Fonts[0], "^", glyph.fgv + (new Vector2(-3, -5)), glyph.fg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                        }
                        else
                        {
                            Sprites.DrawString(glyph.spriteFont, glyph.symbol, glyph.fgv, glyph.fg);
                        }
                       
                    }
                }
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
}
