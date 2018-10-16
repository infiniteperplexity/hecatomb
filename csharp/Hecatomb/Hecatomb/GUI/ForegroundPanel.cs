/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/15/2018
 * Time: 8:23 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
	/// Description of ForegroundPanel.
	/// </summary>
	public class ForegroundPanel : TextPanel
	{
		bool Active;
		List<string> notext;
		List<string> CurrentText;
		TextColors CurrentColors;
		
		public ForegroundPanel(): base()
		{
			X0 = 0;
			Y0 = 0;
			Active = false;
			notext = new List<String>();
		}
		
		public override void Initialize()
		{
			Height = Game.StatusPanel.Y0 + Game.StatusPanel.Height;
			Width = Game.MenuPanel.X0 + Game.MenuPanel.Width;
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
			if (Active)
			{
				// there isn't any...?
				base.DrawLines(CurrentText, CurrentColors);
			}
		}
		
		public void Splash(List<string> lines)
		{
			Splash(lines, nocolors);
		}
		
		public void Splash(List<string> lines, TextColors colors)
		{
			Active = true;
			Dirty = true;
			CurrentText = lines;
			CurrentColors = colors;
		}
		
		public void Reset()
		{
			CurrentText = notext;
			CurrentColors = nocolors;
			Active = false;
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
		}
	}
}
