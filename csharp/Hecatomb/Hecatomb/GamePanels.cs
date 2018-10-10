﻿/*
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
		public int X0;
		public int Y0;

		
		public GamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites)
		{
			Graphics = graphics;
			Sprites = sprites;
		}
		
		public virtual void DrawContent() {}
	}
	public class MainGamePanel : GamePanel
	{
		List<SpriteFont> Fonts;
		Dictionary<char, Vector2> measureCache;
		public int Size;
		public int Padding;
		public Texture2D BG;
		public SparseArray3D<Particle> Particles;
		public HashSet<Coord> OldDirtyTiles;
		public HashSet<Coord> NextDirtyTiles;
		
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
			Particles = new SparseArray3D<Particle>(Constants.WIDTH, Constants.HEIGHT, Constants.DEPTH);
			OldDirtyTiles = new HashSet<Coord>();
			NextDirtyTiles = new HashSet<Coord>();
		}
		
//		public static Coord MouseToTile(int x, int y)
//		{
//			
//		}
		
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
			OldDirtyTiles.UnionWith(NextDirtyTiles);
			foreach (Coord c in OldDirtyTiles)
        	{
				var glyph = Tiles.GetGlyph(c.x, c.y, c.z);
				Coord cc = Tiles.ToCamera(c);
				DrawGlyph(cc.x, cc.y, glyph.Item1, glyph.Item2, glyph.Item3);
        	}
			var swap = OldDirtyTiles;
			OldDirtyTiles = NextDirtyTiles;
			NextDirtyTiles = swap;
			NextDirtyTiles.Clear();
		}

		public override void DrawContent()
		{
			GameCamera Camera = Game.Camera;
			var grid = Game.World.Tiles;
			int z = Camera.z;
			Tuple<char, string, string> glyph;
			for (int i=0; i<Camera.Width; i++) {
		    	for (int j=0; j<Camera.Height; j++) {
					int x = i + Camera.XOffset;
					int y = j + Camera.YOffset;
					glyph = Tiles.GetGlyph(x, y, z);
					DrawGlyph(i, j, glyph.Item1, glyph.Item2, glyph.Item3);
		    	}
			}
		}
		public void DrawGlyph(int i, int j, char c, string fg, string bg)
		{
			string s = c.ToString();
			Vector2 measure = measureChar(c);
			int xOffset = 11-(int) measure.X/2;
			int yOffset = 10-(int) measure.Y/2;
			Color cfg = (fg==null) ? Color.Red : Game.Colors[fg];
			Color cbg = (bg==null) ? Color.Red : Game.Colors[bg];
			var vbg = new Vector2(Padding+(1+i)*(Size+Padding),Padding+(1+j)*(Size+Padding));
			var vfg = new Vector2(xOffset+Padding+(1+i)*(Size+Padding), yOffset+Padding+(1+j)*(Size+Padding));
			Sprites.Draw(BG, vbg, cbg);
			if (c!=default(char))
			{
				Sprites.DrawString(getFont(c), s, vfg, cfg);
			}
		}
		
		protected Vector2 measureChar(char c)
		{	
			if (c==default(char))
			{
				return new Vector2(0,0);
			}
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
	
	public abstract class TextPanel : GamePanel
	{
		public SpriteFont Font;
		public int Width;
		public int Height;
		public int Size;
		public int Spacing;
		public Dictionary<Tuple<int, int>, string> nocolors;
		public TextPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
			nocolors = new Dictionary<Tuple<int, int>, string>();
		}
		public void DrawLines(List<string> lines)
		{
			DrawLines(lines, nocolors);
		}
		public void DrawLines(List<string> lines, Dictionary<Tuple<int, int>, string> fgs)
		{
			Vector2 v;
			string[] tokens;
			int p = 0;
			int m;
			for (int i=0; i<lines.Count; i++)
			{
				int totalWidth = 0;
				Color fg = Color.White;
				Color bg = Color.White;
				v = new Vector2(X0, Y0 + p*Size);
				tokens = lines[i].Split();
				for (int j=0; j<tokens.Length; j++)
				{
					var ij = new Tuple<int, int>(i, j);
					if (fgs.ContainsKey(ij))
					{
						fg = Game.Colors[fgs[ij]];
					}
					m = Spacing + (int) Font.MeasureString(tokens[j]).X;
					if (j>0 && totalWidth+m >= Width-Size)
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
	
	public class MenuGamePanel : TextPanel
	{
		public List<string> middleLines;
		
		public MenuGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Width = 400;
			Size = 16;
			Spacing = 8;
			int size = Game.MainPanel.Size;
			int padding = Game.MainPanel.Padding;
			X0 = padding+(2+Game.Camera.Width)*(size+padding);
			Y0 = padding+(size+padding);
			middleLines = new List<string>() {
				" ",
				"--------------------------------------"
			};
		}
		
		public override void DrawContent()
		{
			DrawLines(Game.Controls.MenuText.Concat(middleLines).ToList(), Game.Controls.TextColors);
		}
	}
	
	
	public class StatusGamePanel : TextPanel
	{
		public StatusGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Height = 100;
			Size = 16;
			Spacing = 8;
			Width = Game.MenuPanel.X0 + Game.MenuPanel.Width;
			int size = Game.MainPanel.Size;
			int padding = Game.MainPanel.Padding;
			X0 = padding+(size+padding);
			Y0 = padding+(2+Game.Camera.Width)*(size+padding);
		}
		
		public override void DrawContent()
		{
			Player p = Game.World.Player;
        	string txt = String.Format("X:{0} Y:{1} Z:{2}", p.x, p.y, p.z);
        	DrawLines(new List<string>() {txt});
		}
	}
}