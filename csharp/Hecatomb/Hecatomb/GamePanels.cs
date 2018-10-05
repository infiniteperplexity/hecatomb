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

namespace Hecatomb
{

	public abstract class GamePanel
	{
		protected GraphicsDeviceManager Graphics;
		protected SpriteBatch Sprites;
		protected int X0;
		protected int Y0;
		
		public GamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites)
		{
			Graphics = graphics;
			Sprites = sprites;
		}
	}
	public class MainGamePanel : GamePanel
	{
		List<SpriteFont> Fonts;
		Dictionary<char, Vector2> measureCache;
		public int Size;
		public int Padding;
		Texture2D BG;
		public MainGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			X0 = 0;
			Y0 = 0;
			Size = 18;
			Padding = 3;
			BG = new Texture2D(graphics.GraphicsDevice, Size+Padding, Size+Padding);
			Color[] bgdata = new Color[(Size+Padding)*(Size+Padding)];
			for(int i=0; i<bgdata.Length; ++i)
			{
				bgdata[i] = Color.White;
			}
			BG.SetData(bgdata);
			Fonts = new List<SpriteFont>();
			string[] fonts = new string[] {"NotoSans", "NotoSansSymbol", "NotoSansSymbol2"};
			foreach(string s in fonts)
			{
				SpriteFont font = Game.MyContentManager.Load<SpriteFont>(s);
				Fonts.Add(font);
			}
			measureCache = new Dictionary<char, Vector2>();
		}
		public void DrawGlyph(int i, int j, char c, string fg, string bg)
		{
			string s = c.ToString();
			Vector2 measure = measureChar(c);
			int xOffset = 11-(int) measure.X/2;
			int yOffset = 10-(int) measure.Y/2;
			Color cfg = Game.Colors[fg];
			Color cbg = Game.Colors[bg];
			var vbg = new Vector2(Padding+(1+i)*(Size+Padding),Padding+(1+j)*(Size+Padding));
			var vfg = new Vector2(xOffset+Padding+(1+i)*(Size+Padding), yOffset+Padding+(1+j)*(Size+Padding));
			Sprites.Draw(BG, vbg, cbg);
			Sprites.DrawString(getFont(c), s, vfg, cfg);
		}
		
		protected Vector2 measureChar(char c)
		{	
			if (measureCache.ContainsKey(c))
			{
				return measureCache[c];
			}
			string s = c.ToString();
			foreach (SpriteFont f in Fonts)
			{
				if (f.GetGlyphs().ContainsKey(c))
				{
					measureCache[c] = f.MeasureString(c.ToString());
					return measureCache[c];
				}
			}
			throw new InvalidOperationException(String.Format("No font found for symbol {0}",c));
		}
		
		public SpriteFont getFont(char c)
		{
			foreach (SpriteFont f in Fonts)
			{
				if (f.GetGlyphs().ContainsKey(c))
				{
					return f;
				}
			}
			throw new InvalidOperationException(String.Format("No font found for symbol {0}",c));
		}
	}
	
	
	public class MenuGamePanel : GamePanel
	{
		public SpriteFont Font;
		public int Width;
		public int Height;
		public int Size;
		public int Spacing;
		// each line of text
		public List<string> Lines;
		// color formatting, by line number and token number
		public Dictionary<Tuple<int, int>, Color> FG;
		public Dictionary<Tuple<int, int>, Color> BG;
		
		public MenuGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Width = 400;
			Size = 18;
			Spacing = 8;
			int size = Game.MainPanel.Size;
			int padding = Game.MainPanel.Padding;
			X0 = padding+(2+Game.Camera.Width)*(size+padding);
			Y0 = padding+(size+padding);
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
			Lines = new List<string>();
			Lines.Add("Hello world.");
			Lines.Add("0 1 2 3 4 5 6 7 8 9 a b c d e f g h i j k l m n o p q r");
			FG = new Dictionary<Tuple<int, int>, Color>();
			FG[new Tuple<int, int>(1,5)] = Color.Red;
			BG = new Dictionary<Tuple<int, int>, Color>();
		}
		
		public void Draw()
		{
			Vector2 v;
			string[] tokens;
			int p = 0;
			int m;
			for (int i=0; i<Lines.Count; i++)
			{
				int totalWidth = 0;
				Color fg = Color.White;
				Color bg = Color.White;
				v = new Vector2(X0, Y0 + p*Size);
				tokens = Lines[i].Split();
				for (int j=0; j<tokens.Length; j++)
				{
					var ij = new Tuple<int, int>(i, j);
					if (FG.ContainsKey(ij))
					{
						fg = FG[ij];
					}
					m = Spacing + (int) Font.MeasureString(tokens[j]).X;
					if (totalWidth + m >= Width - Size)
					{
						p+=1;
						totalWidth = 0;
					}
					v = new Vector2(X0 + totalWidth, Y0 + p*Size);
					Sprites.DrawString(Font, tokens[j], v, fg);
					totalWidth = totalWidth + m;
				}
				p+=1;
			}
		}
		
		
	}
	
	public class StatusGamePanel : GamePanel
	{
		public SpriteFont Font;
		public int Width;
		public int Height;
		public StatusGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Height = 100;
			int Size = Game.MainPanel.Size;
			int Padding = Game.MainPanel.Padding;
			X0 = Padding+(Size+Padding);
			Y0 = Padding+(2+Game.Camera.Width)*(Size+Padding);
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
		}
	}
}



//        	int xOffset = Padding+(2+Camera.Width)*(Size+Padding);
//        	int yOffset = ;
//        	var vfg = new Vector2(xOffset, yOffset);
//        	s.DrawString(textFont, "Esc: System view.", vfg, Color.White);
//        	int xOffset = Padding+(Size+Padding);
//        	int yOffset = Padding+(2+Camera.Width)*(Size+Padding);
//        	var vfg = new Vector2(xOffset, yOffset);
//        	Player p = Game.World.Player;
//        	string txt = String.Format("X: {0} Y:{1} Z:{2}", p.x, p.y, p.z);
//        	s.DrawString(textFont, txt, vfg, Color.White);