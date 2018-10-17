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
		public List<ParticleEmitter> Emitters;
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
			Emitters = new List<ParticleEmitter>();
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
		public TextColors nocolors;
		public TextPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			Font = Game.MyContentManager.Load<SpriteFont>("PTMono");
			nocolors = new TextColors();
		}
		public void DrawLines(List<string> lines)
		{
			DrawLines(lines, nocolors);
		}
		public void DrawLines(List<string> lines, TextColors fgs)
		{
			Vector2 v;
			string[] tokens;
			int p = 0;
			int m;
			for (int i=0; i<lines.Count; i++)
			{
				int totalWidth = 0;
				string fg = "white";
//				string bg = "white";
				v = new Vector2(X0, Y0 + p*Size);
				tokens = lines[i].Split();
				for (int j=0; j<tokens.Length; j++)
				{
					fg = fgs[i, j] ?? fg;
					m = Spacing + (int) Font.MeasureString(tokens[j]).X;
					if (j>0 && totalWidth+m >= Width-Size)
					{
						p+=1;
						totalWidth = 0;
					}
					v = new Vector2(X0 + totalWidth, Y0 + p*Size);
					Sprites.DrawString(Font, tokens[j], v, Game.Colors[fg]);
					totalWidth = totalWidth + m;
				}
				p+=1;
			}
		}
	}
}