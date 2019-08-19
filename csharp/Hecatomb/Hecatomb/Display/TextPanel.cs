using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    public abstract class TextPanel : GamePanel
    {
        public SpriteFont Font;
        public int Width;
        public int Height;
        public int LeftMargin;
        public int TopMargin;
        public int RightMargin;
        public int Size;
        public int Spacing;
        public TextPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
        {
            Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
            Size = 16;
            Spacing = 9;
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

            //if (this is StatusGamePanel)
            //{
            //    foreach (var key in lines[0].Colors.Keys)
            //    {
            //        Debug.WriteLine(key);
            //        Debug.WriteLine(lines[0].Colors[key]);
            //    }
            //}

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
                    if (text.Substring(j,1)==" ")
                    {
                        for (int k=1; k<text.Length-j; k++)
                        {
                            if (text.Substring(j+k,1)!=" ")
                            {
                                // I have no idea if this spacing is even right
                                if (x >= ((Width - RightMargin) / Spacing) - 8)
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
                    v = new Vector2(X0 + LeftMargin + x * Spacing, Y0 + TopMargin + y * Size);
                    Sprites.DrawString(Font, text.Substring(j, 1), v, Game.Colors[fg]);
                    x += 1;
                }
            }
        }
        public void OldDrawLines(List<ColoredText> lines)
        {
            Vector2 v;
            int x = 0;
            int y = 0;
            int p = 0;
            int spaces = 0;
            string fg = "white";
            string text = "";
            SortedList<int, string> colors;
            
            //int spaces = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                spaces = 0;
                text = lines[i].Text;
                colors = lines[i].Colors;
                // advance by one line for every line of input
                y++;
                // return to left margin
                x = 0;
                p = 0;
                // initialize to white
                fg = "white";
                for (int j = 0; j < text.Length; j++)
                {
                    if (colors.ContainsKey(j))
                    {
                        fg = colors[j];
                    }
                    if (x + j - p + 1 >= (Width / Spacing) - 2)
                    {
                        spaces = 0;
                        x = -1;
                        y++;
                    }
                    if (text[j] == ' ')
                    {
                        if (j > 0 && text[j - 1] == ' ' && x == -1)
                        {
                            x -= 2;
                        }
                        else
                        {

                        } 
                        v = new Vector2(X0 + LeftMargin + x * Spacing - spaces, Y0 + TopMargin + y * Size);
                        Sprites.DrawString(Font, text.Substring(p, j - p), v, Game.Colors[fg]);
                        spaces += 1;
                        x += (j - p) + 1;
                        p = j;

                    }
                    else if (j == text.Length - 1)
                    {
                        v = new Vector2(X0 + LeftMargin + x * Spacing - spaces, Y0 + TopMargin + y * Size);
                        Sprites.DrawString(Font, text.Substring(p), v, Game.Colors[fg]);
                    }
                }
            }
        }
    }
}
