/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 10:40 AM
 */
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
	/// <summary>
	/// Description of StatusPanel.
	/// </summary>
	public class StatusGamePanel : TextPanel
	{
		TextColors statusColors;
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
			statusColors = new TextColors(0,3,"yellow");
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
			string txt = String.Format(
				"X:{0} Y:{1} Z:{2}                                     {3}",
				p.X.ToString().PadRight(3),
				p.Y.ToString().PadRight(3),
				p.Z.ToString().PadRight(3),
			    (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "Paused" : "      "
			);
        	DrawLines(new List<string>() {txt}, statusColors);
		}
	}
}
