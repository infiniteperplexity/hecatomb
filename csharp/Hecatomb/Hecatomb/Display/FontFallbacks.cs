/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/1/2018
 * Time: 1:38 PM
 */
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of FontHandler.
	/// </summary>
	public class FontFallbacks
	{
		List<SpriteFont> Fonts;
		Dictionary<char, Vector2> measureCache;
		public FontFallbacks(params string[] fonts)
		{
			Fonts = new List<SpriteFont>();
			foreach(string s in fonts)
			{
				SpriteFont font = Game.MyContentManager.Load<SpriteFont>(s);
				Fonts.Add(font);
			}
			measureCache = new Dictionary<char, Vector2>();
		}
		
		public SpriteFont GetFont(char c)
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
		
		public Vector2 MeasureString(string s)
		{
			return Fonts[0].MeasureString(s);
		}
		
		public Vector2 MeasureChar(char c)
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
		
		// it's 10, 18 for size 12 PT Mono
		private List<string> wrapText(string s, int charsWidth)
		{
			List<string> list = new List<string>();
			return list;
		}
	}
}
