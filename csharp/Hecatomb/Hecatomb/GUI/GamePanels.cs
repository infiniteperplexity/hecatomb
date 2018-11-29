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
        private List<(string, string, int)> buffer;
		public TextPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
            buffer = new List<(string, string, int)>();
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
		}
        public void DrawLines(List<ColoredText> lines)
        {
            Vector2 v;
            int displayRow = 0;
            int displayColumn = 0;
            int idx = 0;
            string text = "";
            string fg = "white";
            SortedList<int, string> colors;
            buffer.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                // advance by one line for every line of input
                displayRow++;
                text = lines[i].Text;
                colors = lines[i].Colors;
                // return to left margin
                displayColumn = 0;
                // where the last token on this line ended
                idx = 0;
                // set color to the first color or white
                fg = (colors.ContainsKey(0)) ? colors[0] : "white";
                // step through each character
   //             for (int j = 0; j < lines[i].Text.Length; j++)
   //             {
   //                 if (colors.ContainsKey(j))
   //                 {
   //                     buffer.Add((text.Substring(idx, j), fg, displayColumn));
   //                     idx = j;
   //                     fg = lines[i].Colors[j];
   //                 }
   //                 else if (j > Width - Size)
   //                 {
   //                     displayRow++;
   //                     buffer.Add((text.Substring(k, j), fg, k));
   //                     k = j;
   //                     p++;
   //                 }
   //             }

   //                 do
   //                 {

   //                 } while ()
   //             }
   //             else
   //             {
   //                 foreach (int idx in lines[i].Colors.Keys)
   //                 {

   //                 }
   //             }
			//	int totalWidth = 0;
			//	string fg = "white";
			//	v = new Vector2(X0, Y0 + p*Size);
   //             List<>
   //             string text = lines[i].Text;
   //             while (text.Length>= Width - Size)
   //             {

   //             }
			}
		}
	}
}

//v = new Vector2(X0 + totalWidth, Y0 + p* Size);
//Sprites.DrawString(Font, tokens[j], v, Game.Colors[fg]);