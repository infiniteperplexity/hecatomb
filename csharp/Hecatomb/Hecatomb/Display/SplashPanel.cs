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
using System.Threading;


namespace Hecatomb
{
	/// <summary>
	/// Description of ForegroundPanel.
	/// </summary>
	public class SplashPanel : TextPanel
	{
		public bool Active;
		List<ColoredText> CurrentText;
		
		public SplashPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
		{
			X0 = 35;
			Y0 = 150;
            LeftMargin = 2 * Size;
            Active = false;
		}
		
		public void Initialize()
		{
			Height = Size*13;
			Width = Size*31;
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
            // eventually want some kind of brief freeze to keep from instantly closing this
            Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            Vector2 v;
            v = new Vector2(X0, Y0);
            Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
            for (var i=1; i<=11; i++)
            {
                v = new Vector2(X0, Y0 + Size * i);
                Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
                v = new Vector2(X0+Size*30+7, Y0 + Size * i);
                Sprites.DrawString(Font, "#", v, Game.Colors["yellow"]);
            }
            v = new Vector2(X0, Y0 + Size * 12);
            Sprites.DrawString(Font, new string('=', 55), v, Game.Colors["yellow"]);
            DrawLines(CurrentText);
        }
		
		public void Splash(List<ColoredText> lines, bool frozen = false)
		{
            Active = true;
            Dirty = true;
            if (!frozen)
            {
                Game.Controls.Set(new FrozenControls());
                Thread thread = new Thread(()=>
                {
                    Thread.Sleep(500);
                    Game.Controls.SetWithoutRedraw(new SplashControls());
                });
                thread.Start();
            }
            else
            {
                Game.Controls.Set(new FrozenControls());
            }
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
