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
    public class MainPanel : InterfacePanel
    {
        List<SpriteFont> Fonts;
        Dictionary<char, ValueTuple<Vector2, SpriteFont>> fontCache;
        public HashSet<Coord> OldDirtyTiles;
        public HashSet<Coord> NextDirtyTiles;
        public bool IntroState;
        public string[] IntroLines;
        public MainPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            IntroLines = System.IO.File.ReadAllLines(@"Content/ASCII_icon.txt");
            IntroState = false;
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
            OldDirtyTiles = new HashSet<Coord>();
            NextDirtyTiles = new HashSet<Coord>();
        }

        public void DirtifyTile(int x, int y, int z)
        {
            NextDirtyTiles.Add(new Coord(x, y, z));
        }
        public void DirtifyTile(Coord c)
        {
            NextDirtyTiles.Add(c);
        }

        public void DrawDirty()
        {
            if (Game.World == null || !World.WorldSafeToDraw)
            {
                return;
            }
            OldDirtyTiles.UnionWith(NextDirtyTiles);
            foreach (Coord c in OldDirtyTiles)
            {
                var glyph = Tiles.GetGlyph(c.X, c.Y, Game.Camera.Z);

                int x = c.X - Game.Camera.XOffset;
                int y = c.Y - Game.Camera.YOffset;
                if (x >= 0 && x < Game.Camera.Width && y >= 0 && y < Game.Camera.Height)
                {
                    DrawGlyph(x, y, glyph.Item1, glyph.Item2, glyph.Item3);
                }
            }
            var swap = OldDirtyTiles;
            OldDirtyTiles = NextDirtyTiles;
            NextDirtyTiles = swap;
            NextDirtyTiles.Clear();
        }
        public override void Draw()
        {
            Game.Graphics.GraphicsDevice.Clear(Color.Black);
            if (IntroState)
            {
                for (int j = 0; j < IntroLines.Length; j++)
                {
                    var line = IntroLines[j];
                    for (int i = 0; i < line.Length; i++)
                    {
                        var sym = line[i];
                        string fg = "white";
                        string bg = "black";
                        if (sym == ' ')
                        {
                            continue;
                        }
                        else if (sym == 'z')
                        {
                            fg = "lime green";
                        }
                        else if (sym == '@')
                        {
                            fg = "magenta";
                        }
                        DrawGlyph(i / 2, j, sym, fg, bg);
                    }
                }
                return;
            }
            if (Game.World == null || !World.WorldSafeToDraw)
            {
                for (int i = 0; i < Game.Camera.Width; i++)
                {
                    for (int j = 0; j < Game.Camera.Height; j++)
                    {
                        Game.Sprites.Draw(BG, new Vector2(X0 + XPad + (1 + i) * (CharWidth + XPad), Y0 + YPad + (1 + j) * (CharHeight + YPad)), Color.Black);
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
            var vbg = new Vector2(X0 + XPad + i * (CharWidth + XPad), Y0 + YPad + j * (CharHeight + YPad));
            var vfg = new Vector2(X0 + xOffset + XPad + i * (CharWidth + XPad), Y0 + yOffset + YPad + j * (CharHeight + YPad));
            //var vbg = new Vector2(Padding + (i) * (Size + Padding), Padding + (j) * (Size + Padding));
            //var vfg = new Vector2(xOffset + Padding + (i) * (Size + Padding), yOffset + Padding + (j) * (Size + Padding));
            Game.Sprites.Draw(BG, vbg, cbg);
            if (c == '\u02C7')
            {
                Game.Sprites.DrawString(Fonts[0], "^", vfg + (new Vector2(-2, 0)), cfg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
            }
            else if (c == '\u2235')
            {
                Game.Sprites.DrawString(Fonts[1], "\u26EC", vfg + (new Vector2(-3, -5)), cfg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
            }
            else
            {
                Game.Sprites.DrawString(font, s, vfg, cfg);
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
