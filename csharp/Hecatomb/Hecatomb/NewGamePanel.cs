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
                                if (x >= (((PixelWidth - CharWidth * LeftMargin - CharWidth * RightMargin) / CharWidth) - 8))
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
                    v = new Vector2(X0 + CharWidth * LeftMargin + x * CharWidth, TopMargin + Y0 + y * CharHeight);
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
    }

    public class MainPanel : InterfacePanel
    {
        List<SpriteFont> Fonts;
        Dictionary<char, ValueTuple<Vector2, SpriteFont>> fontCache;
        public HashSet<Coord> OldDirtyTiles;
        public HashSet<Coord> NextDirtyTiles;
        public MainPanel(int x, int y, int w, int h) : base(x, y, w, h)
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
            if (Game.World == null || !World.WorldSafeToDraw)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j < 25; j++)
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

    public class InstructionsPanel : InterfacePanel
    {
        public InstructionsPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }


        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
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

    public class ScrollPanel : InterfacePanel
    {
        public int SelectedMessage;
        public ScrollPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
            Dirty = true;
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < Game.World.GetState<MessageHandler>().MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
            Dirty = true;
        }

        // this should actually be on the message handler
        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            Game.World.GetState<MessageHandler>().MessageHistory.Insert(0, ct);
            while (Game.World.GetState<MessageHandler>().MessageHistory.Count > MaxArchive)
            {
                Game.World.GetState<MessageHandler>().MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
        }


        public override void Draw()
        {
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            string txt;
            if (Game.World != null)
            {
                Creature p = Game.World.Player;
                var (X, Y, Z) = Game.World.Player;
                if (Game.Controls is CameraControls)
                {
                    X = Game.Camera.XOffset + 12;
                    Y = Game.Camera.YOffset + 12;
                    Z = Game.Camera.Z;
                }
                TurnHandler t = Game.World.Turns;
                string sanity = Game.World.Player.GetComponent<SpellCaster>().Sanity.ToString().PadLeft(3, '0') + '/' + Game.World.Player.GetComponent<SpellCaster>().GetCalculatedMaxSanity().ToString().PadLeft(3, '0');
                string x = X.ToString().PadLeft(3, '0');
                string y = Y.ToString().PadLeft(3, '0');
                string z = Z.ToString().PadLeft(3, '0');
                string paused = (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "{yellow}Paused" : "      ";
                string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
                txt = $"Sanity:{sanity}  X:{x} Y:{y} Z:{z} {time}   {paused}";
                int MaxVisible = Math.Min(Game.World.GetState<MessageHandler>().MessageHistory.Count, 4);
                List<ColoredText> list = new List<ColoredText> { txt };
                list = list.Concat(Game.World.GetState<MessageHandler>().MessageHistory.GetRange(SelectedMessage, MaxVisible)).ToList();
                if (list.Count > 1 && list[1].Colors.Count == 0)
                {
                    list[1] = new ColoredText(list[1].Text, "cyan");
                }
                DrawLines(list);
            } 
        }
    }

    public class OtherMenuPanel : InterfacePanel
    {
        public OtherMenuPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }
        public override void Draw()
        {
            if (Game.ForegroundPanel.Active)
            {
                // I don't know why this panel needs this and the others don't
                return;
            }
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            var lines = new List<ColoredText>();
            if (Game.World?.Player != null)
            {
                lines.Add($"{Game.World.Player.Describe()}");
                lines.Add($"Controls {Game.World.GetState<TaskHandler>().Minions.Count} minions.");
                var stored = new List<Dictionary<string, int>>();
                var structures = Structure.ListStructures();
                foreach (Structure s in structures)
                {
                    stored.Add(s.GetStored());
                }
                var total = Item.CombinedResources(stored);
                if (total.Count > 0)
                {
                    lines.Add(" ");
                    lines.Add("Stored resources:");
                    foreach (var res in total.Keys)
                    {
                        lines.Add("- " + Resource.Format((res, total[res])));
                    }
                }
                DrawLines(lines);
            }
        }
    }

    public class NewSplashPanel : InterfacePanel
    {
        List<ColoredText> CurrentText;
        public NewSplashPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {
            Zindex = 1;
            LeftMargin = 2;
            RightMargin = 2;
            Active = false;
            List<ColoredText> CurrentText = new List<ColoredText>();
        }

        public override void Draw()
        {
            // eventually want some kind of brief freeze to keep from instantly closing this
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            Vector2 v;
            v = new Vector2(X0, Y0);
            Game.Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
            for (var i = 1; i <= 11; i++)
            {
                v = new Vector2(X0, Y0 + CharHeight * i);
                Game.Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
                v = new Vector2(X0 + CharHeight * 30 + 7, Y0 + CharHeight * i);
                Game.Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
            }
            v = new Vector2(X0, Y0 + CharHeight * 12);
            Game.Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
            DrawLines(CurrentText);
        }

        public void Splash(List<ColoredText> lines, bool frozen = false)
        {
            Active = true;
            Dirty = true;
            if (!frozen)
            {
                Game.Controls.Set(new FrozenControls());
                Thread thread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    Game.Controls.SetWithoutRedraw(new SplashControls());
                });
                thread.Start();
            }
            else
            {
                Game.Controls.Set(new FrozenControls());
            }
            CurrentText = lines;
        }

        public void Reset()
        {
            Active = false;
            Game.MainPanel.Dirty = true;
            Game.MenuPanel.Dirty = true;
            Game.OtherPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
        }
    }

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
            Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            DrawLines(CurrentText);
        }
        public void Splash(List<ColoredText> lines, bool frozen = false)
        {
            Active = true;
            Dirty = true;
            if (!frozen)
            {
                Game.Controls.Set(new SplashControls());
            }
            else
            {
                Game.Controls.Set(new FrozenControls());
            }
            CurrentText = lines;
        }

        public void Reset()
        {
            Active = false;
            Game.MainPanel.Dirty = true;
            Game.MenuPanel.Dirty = true;
            Game.OtherPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
        }
    }
}