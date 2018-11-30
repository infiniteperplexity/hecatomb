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

	public abstract class GamePanel
	{
		protected GraphicsDeviceManager Graphics;
		protected SpriteBatch Sprites;
		public Texture2D BG;
		public bool Dirty;
		public int X0;
		public int Y0;

		
		public GamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites)
		{
			Graphics = graphics;
			Sprites = sprites;
			Dirty = true;
		}
		
		public virtual void DrawContent() {}
	}
	
	
	public abstract class TextPanel : GamePanel
	{
		public SpriteFont Font;
		public int Width;
		public int Height;
		public int Size;
		public int Spacing;
		public TextPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
		}
        public void DrawLines(List<ColoredText> lines)
        {
            Vector2 v;
            int x = 0;
            int y = 0;
            int p = 0;
            string fg = "white";
            string text = "";
            SortedList<int, string> colors;
            for (int i = 0; i < lines.Count; i++)
            {
                text = lines[i].Text;
                colors = lines[i].Colors;
                // advance by one line for every line of input
                y++;
                // return to left margin
                x = 0;
                p = 0;
                // initialize to white
                fg = "white";
                for (int j=0; j<text.Length; j++)
                {
                    if (colors.ContainsKey(j))
                    {
                        fg = colors[j];
                    }
                    x++;
                    if (x >= Width - Size)
                    {
                        x = 0;
                        y++;
                    }
                    if (text[j]==' ')
                    {
                        v = new Vector2(X0 + x * Spacing, Y0 + y * Size);
                        Sprites.DrawString(Font, text.Substring(p, j-p), v, Game.Colors[fg]);
                        p = j;
                    }
                }
			}
		}
	}
}