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
	/// <summary>
	/// Description of MenuPanel.
	/// </summary>
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
		
		public void Initialize()
		{
			Height = Game.StatusPanel.Y0 + Game.StatusPanel.Height;
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
			var c = Game.Controls;
            var tutorial = Game.World.GetTracker<TutorialTracker>();
            List<string> MenuTop = (tutorial.Visible) ? tutorial.Current.ControlText : c.MenuTop;
            TextColors TopColors = (tutorial.Visible) ? tutorial.Current.ControlColors : c.TopColors;
            List<string> MenuMiddle = (tutorial.Visible) ? tutorial.Current.InstructionsText : c.MenuMiddle;
            TextColors MiddleColors = (tutorial.Visible) ? tutorial.Current.InstructionsColors : c.MiddleColors;
            var text = MenuTop.Concat(middleLines).ToList();
			if (MenuMiddle.Count>0)
			{
				text.Add(" ");
			}
			int i0 = text.Count;
			text = text.Concat(MenuMiddle).ToList();
			if (c.MenuBottom.Count>0)
			{
				text.Add(" ");
			}
			int i1 = text.Count;
			text = text.Concat(c.MenuBottom).ToList();
			var colors = new TextColors();
			foreach (var key in TopColors.Keys)
			{
				colors[key.Item1, key.Item2] = TopColors[key.Item1, key.Item2];
			}
			foreach (var key in MiddleColors.Keys)
			{
				colors[key.Item1+i0, key.Item2] = MiddleColors[key.Item1, key.Item2];
			}
			foreach (var key in c.BottomColors.Keys)
			{
				colors[key.Item1+i1, key.Item2] = c.BottomColors[key.Item1, key.Item2];
			}
			DrawLines(text, colors);
		}
	}
}
