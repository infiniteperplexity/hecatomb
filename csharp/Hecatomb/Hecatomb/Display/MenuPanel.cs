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
    using static HecatombAliases;
    /// <summary>
    /// Description of MenuPanel.
    /// </summary>
    public class MenuGamePanel : TextPanel
    {
        public List<ColoredText> middleLines;

        public MenuGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
        {
            Width = 400;
            int size = Game.MainPanel.Size;
            int padding = Game.MainPanel.Padding;
            X0 = padding + (2 + Game.Camera.Width) * (size + padding);
            Y0 = padding + (size + padding);
            middleLines = new List<ColoredText>() {
                " ",
                "--------------------------------------"
            };
        }

        public void Initialize()
        {
            Height = Game.StatusPanel.Y0 + Game.StatusPanel.Height;
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
            Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            var c = Game.Controls;
            c.RefreshContent();
            var tutorial = (Time.Frozen || Game.World==null) ? null : Game.World.GetState<TutorialHandler>();

            List<ColoredText> MenuTop = c.MenuTop;
            List<ColoredText> MenuMiddle = c.MenuMiddle;
            if (!Time.Frozen && tutorial!=null && tutorial.Visible)
            {
                if (!tutorial.Current.RequiresDefaultControls || Game.Controls == Game.DefaultControls)
                {
                    MenuTop = tutorial.Current.ControlText;
                    MenuMiddle = tutorial.Current.InstructionsText;
                }
                else if (Game.Controls == Game.CameraControls)
                {
                    MenuMiddle = tutorial.OffTutorialCamera;
                }
                else
                {
                    MenuMiddle = tutorial.OffTutorialText;
                }
            }
            List<ColoredText> text;
            if (Game.World == null)
            {
                text = MenuTop.ToList();
            }
            else
            {
                text = MenuTop.Concat(middleLines).ToList();
            }
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
			DrawLines(text);
		}
	}
}
