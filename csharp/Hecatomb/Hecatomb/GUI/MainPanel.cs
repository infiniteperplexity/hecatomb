﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/10/2018
 * Time: 12:42 PM
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
public class MainGamePanel : GamePanel
	{
		List<SpriteFont> Fonts;
		Dictionary<char, Vector2> measureCache;
		public int Size;
		public int Padding;
		public SparseJaggedArray3D<Particle> Particles;
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
			Particles = new SparseJaggedArray3D<Particle>(Constants.WIDTH, Constants.HEIGHT, Constants.DEPTH);
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
}
