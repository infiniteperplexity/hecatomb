﻿/*
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
		List<ColoredText> CurrentText;
		
		public ForegroundPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			X0 = 0;
			Y0 = 0;
			Active = false;
		}
		
		public void Initialize()
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
				base.DrawLines(CurrentText);
			}
		}
		
		public void Splash(List<ColoredText> lines)
		{
            Active = true;
            Dirty = true;
            CurrentText = lines;
        }
		
		public void Reset()
		{
			Active = false;
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
		}
	}
}