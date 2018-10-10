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
		
		public void Initialize()
		{
			BG = new Texture2D(Graphics.GraphicsDevice, Width, Height);
			Color[] bgdata = new Color[Width*Height];
			for(int i=0; i<bgdata.Length; ++i)
			{
				bgdata[i] = Color.White;
			}
			BG.SetData(bgdata);
		}
		
		public override void DrawContent()
		{
			Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
			Player p = Game.World.Player;
        	string txt = String.Format("X:{0} Y:{1} Z:{2}", p.x, p.y, p.z);
        	DrawLines(new List<string>() {txt});
		}
	}
}