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
    // a locator class for global interface stuff
    // what if we modified this only with a method called Update?
    static class InterfaceState
    {
        static GamePanel? mainPanel;
        public static GamePanel MainPanel { get => mainPanel!; set => mainPanel = value; }
        static ControlContext? controls;
        public static ControlContext Controls { get => controls!; set => controls = value; }
        public static bool ReadyForInput;

        public static void HandleInput()
        {
            controls!.HandleInput();
        }
        public static void PrepareSprites()
        {
            mainPanel!.PrepareGlyphs();
        }
        public static void DrawInterfacePanels()
        {
            mainPanel!.Draw();
        }
    }

    public class GamePanel
    {
        SpriteBatch Sprites;
        GraphicsDevice Graphics;
        int CharWidth;
        int CharHeight;
        int XPad;
        int YPad;
        Texture2D BG;
        List<SpriteFont> Fonts;
        Dictionary<char, (Vector2, SpriteFont)> fontCache = new Dictionary<char, (Vector2, SpriteFont)>();
        // this is actually kind of an insidious loophole because when you initialize an array 
        DrawableGlyph?[,] NextGlyphs;

        public GamePanel(GraphicsDevice g, SpriteBatch sb, ContentManager c)
        {
            Graphics = g;
            Sprites = sb;
            CharWidth = 18;
            CharHeight = 18;
            XPad = 3;
            YPad = 3;
            BG = new Texture2D(Graphics, CharWidth + XPad, CharHeight + YPad);
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
                SpriteFont font = c.Load<SpriteFont>(s);
                Fonts.Add(font);
            }
            fontCache = new Dictionary<char, (Vector2, SpriteFont)>();
            NextGlyphs = new DrawableGlyph?[25, 25];
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

        public void PrepareGlyphs()
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    // we should probably make worldExists into a nullable field to force this check
                    if (GameState.WorldExists)
                    {
                        var symbol = '.';
                        var bg = Color.Black;
                        var fg = Color.White;
                        var p = GameState.Player;
                        if (!(p is null) && p.X == i && p.Y == j)
                        {
                            symbol = '@';
                            fg = Color.Magenta;
                        }
                        var str = symbol.ToString();
                        Vector2 v = Fonts[0].MeasureString(str);
                        var measure = new Vector2(v.X, -7);
                        int xOffset = 11 - (int)measure.X / 2;
                        int yOffset = (int)measure.Y;
                        var bgv = new Vector2(/*X0 + */XPad + (1 + i) * (CharWidth + XPad), /*Y0 + */YPad + (1 + j) * (CharHeight + YPad));
                        var fgv = new Vector2(/*X0 + */xOffset + XPad + i * (CharWidth + XPad), /*Y0 + */yOffset + YPad + j * (CharHeight + YPad));
                        NextGlyphs[i, j] = new DrawableGlyph(fgv, bgv, Fonts[0], str, fg, bg);
                    }
                    else
                    {
                        
                    }                   
                }
            }
        }

        public void Draw()
        {
            // draw background tiles
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var glyph = NextGlyphs[i, j];
                    if (glyph != null)
                    {
                        Sprites.Draw(BG, glyph.bgv, glyph.bg);
                    }
                }
            }
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var glyph = NextGlyphs[i, j];
                    if (glyph != null)
                    {
                        Sprites.DrawString(glyph.spriteFont, glyph.symbol, glyph.fgv, glyph.fg);
                    }
                }
            }
        }
    }

}
