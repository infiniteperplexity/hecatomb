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

namespace Hecatomb
{
	/// <summary>
	/// Description of FontHandler.
	/// </summary>
	public class FontHandler
	{
		List<SpriteFont> Fonts;
		public FontHandler(params string[] fonts)
		{
			Fonts = new List<SpriteFont>();
			foreach(string s in fonts)
			{
				SpriteFont font = Game.MyContentManager.Load<SpriteFont>(s);
				Fonts.Add(font);
			}
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
	}
}
