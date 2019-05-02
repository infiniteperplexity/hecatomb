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
        public bool Active;
        List<ColoredText> CurrentText;

        public ForegroundPanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
        {
            X0 = 0;
            Y0 = 0;
            Size = 16;
            Spacing = 8;
            Active = false;
        }

        public void Initialize()
        {
            Height = Game.StatusPanel.Y0 + Game.StatusPanel.Height;
            Width = Game.MenuPanel.X0 + Game.MenuPanel.Width;
            BG = new Texture2D(Graphics.GraphicsDevice, Width, Height);
            Color[] bgdata = new Color[Width * Height];
            for (int i = 0; i < bgdata.Length; ++i)
            {
                bgdata[i] = Color.White;
            }
            BG.SetData(bgdata);
        }


        public override void DrawContent()
        {
            Debug.WriteLine("drawing foreground panel1");
            // eventually want some kind of brief freeze to keep from instantly closing this
            Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            DrawLines(CurrentText);
        }

        public void Splash(List<ColoredText> lines, bool frozen=false)
        {
            Active = true;
            Dirty = true;
            if (!frozen)
            {
                Game.Controls.Set(new SplashControls());
            }
            else
            {
                Game.Controls.Set(new FrozenControls());
            }

                for (var i = 0; i < lines.Count; i++)
            {
                lines[i] = "  " + lines[i];
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
