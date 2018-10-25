/*
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
			Particles = new SparseJaggedArray3D<Particle>(Game.World.Width, Game.World.Height, Game.World.Depth);
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
				var glyph = Tiles.GetGlyph(c.X, c.Y, c.Z);
				Coord cc = Tiles.ToCamera(c);
				DrawGlyph(cc.X, cc.Y, glyph.Item1, glyph.Item2, glyph.Item3);
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
			int z = Camera.Z;
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
			int yOffset = (int) measure.Y;
			Color cfg = (fg==null) ? Color.Red : Game.Colors[fg];
			Color cbg = (bg==null) ? Color.Red : Game.Colors[bg];
			var vbg = new Vector2(Padding+(1+i)*(Size+Padding),Padding+(1+j)*(Size+Padding));
			var vfg = new Vector2(xOffset+Padding+(1+i)*(Size+Padding), yOffset+Padding+(1+j)*(Size+Padding));
			Sprites.Draw(BG, vbg, cbg);
			if (c=='\u02C7')
			{
				Sprites.DrawString(getFont(c), "^", vfg+(new Vector2(-2,0)), cfg, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
			}
			else if (c!=default(char))
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
					
					Vector2 v = f.MeasureString(c.ToString());
					if (Fonts[0].GetGlyphs().ContainsKey(c))
		            {
						
						measureCache[c] = new Vector2(v.X, -7);
		            } else if (Fonts[1].GetGlyphs().ContainsKey(c))
		            {

						measureCache[c] = new Vector2(v.X, -17);
						if (c=='\u271D')
						{
							measureCache[c] = new Vector2(v.X, -16);
						}
		            } else if (Fonts[2].GetGlyphs().ContainsKey(c))
		            {
						
						measureCache[c] = new Vector2(v.X, -8);
		            }
		            if (c=='\u2717')
		            {
		            	measureCache[c]= new Vector2(v.X, -9);
		            } 
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
