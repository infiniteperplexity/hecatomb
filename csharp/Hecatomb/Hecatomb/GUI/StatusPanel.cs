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
using System.Linq;

namespace Hecatomb
{
    /// <summary>
    /// Description of StatusPanel.
    /// </summary>
    public class StatusGamePanel : TextPanel
    {
        TextColors statusColors;
        public List<string> MessageHistory;
        public List<string> ColorHistory;

        public StatusGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites) : base(graphics, sprites)
        {
            Height = 100;
            Size = 16;
            Spacing = 8;
            Width = Game.MenuPanel.X0 + Game.MenuPanel.Width;
            int size = Game.MainPanel.Size;
            int padding = Game.MainPanel.Padding;
            X0 = padding + (size + padding);
            Y0 = padding + (2 + Game.Camera.Width) * (size + padding);
            statusColors = new TextColors(0, 6, "yellow", 1, "cyan");
            MessageHistory = new List<string>();
            ColorHistory = new List<string>();
        }

        public void Initialize()
        {
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
            Player p = Game.World.Player;
            TurnHandler t = Game.World.Turns;
            string sanity = Game.World.Player.GetComponent<SpellCaster>().Sanity.ToString().PadLeft(3, '0') + Game.World.Player.GetComponent<SpellCaster>().MaxSanity.ToString().PadLeft(3, '0');
            string x = p.X.ToString().PadLeft(3, '0');
            string y = p.Y.ToString().PadLeft(3, '0');
            string z = p.Z.ToString().PadLeft(3, '0');
            string paused = (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "Paused" : "      ";
            string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
            string txt = $"Sanity:{sanity} X:{x} Y:{y} Z:{z} {time}         {paused}";
            int MaxVisible = 4;
            List<string> list = new List<string> { txt };
            list = list.Concat(MessageHistory.Take(MaxVisible)).ToList();
            DrawLines(list, statusColors);
        }

        public void PushMessage(string s, string color = null)
        {
            int MaxArchive = 100;
            MessageHistory.Insert(0, s);
            ColorHistory.Insert(0, color ?? "white");
            while (MessageHistory.Count > MaxArchive)
            {
                MessageHistory.RemoveAt(MaxArchive);
                ColorHistory.RemoveAt(MaxArchive);
            }
        }
    }
}
