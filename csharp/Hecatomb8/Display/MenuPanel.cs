using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Threading;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class MenuPanel : InterfacePanel
    {
        public MenuPanel(GraphicsDevice g, SpriteBatch sb, ContentManager c, int x, int y, int w, int h) : base(g, sb, c, x, y, w, h)
        {
            LeftMargin = 2;
            RightMargin = 2;
        }

        public override void Prepare()
        {
            if (!Dirty)
            {
                return;
            }
            DrawableLines.Clear();
            if (GameState.World is null)
            {
                return;
            }
            int total = 0;
            int margin = 4 * CharWidth;
            var controls = InterfaceState.Controls;
            for (int i = 0; i < controls.MenuCommands.Count; i++)
            {
                if (GameState.World != null && GameState.World!.GetState<TutorialHandler>().Visible)
                {
                    var text = GetState<TutorialHandler>().Current.MenuCommands[i];
                    var color = (text.Colors.ContainsKey(0)) ? text.Colors[0] : "white";
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    DrawableLines.Add((text, v, InterfaceState.Colors![color]));
                    total += bump;
                }
                else
                {
                    var command = controls.MenuCommands[i];
                    var text = command.text;
                    var color = "white";
                    if (controls.IsMenuSelected(command.command))
                    {
                        color = "yellow";
                    }
                    else if (command.command == "Tutorial" && GameState.World != null && GetState<TutorialHandler>().Visible)
                    {
                        color = "cyan";
                    }
                    else if (!controls.MenuCommandsSelectable)
                    {
                        color = "gray";
                    }
                    else if (command.command == "Log" && GetState<GameLog>().Unread)
                    {
                        color = GetState<GameLog>().UnreadColor;
                    }
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    DrawableLines.Add((text, v, InterfaceState.Colors![color]));
                    total += bump;
                }
            }
            Dirty = false;
        }        
    }
}


// public override void Draw()
//{
//    Game.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
//    int total = 0;
//    int margin = 4 * CharWidth;
//    for (int i = 0; i < Game.Controls.MenuCommands.Count; i++)
//    {
//        if (Game.World != null && Game.World.GetState<TutorialHandler>().Visible)
//        {
//            var text = Game.World.GetState<TutorialHandler>().Current.MenuCommands[i];
//            var color = (text.Colors.ContainsKey(0)) ? text.Colors[0] : "white";
//            int adjust = (i == 0) ? -4 * CharWidth : 0;
//            var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
//            var bump = margin + adjust + text.Length * CharWidth + margin;
//            Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
//            total += bump;
//        }
//        else
//        {
//            var command = Game.Controls.MenuCommands[i];
//            var text = command.Item2;
//            var color = "white";
//            //if (command.Item1 == Game.Controls.SelectedMenuCommand)
//            if (Game.Controls.IsMenuSelected(command.Item1))
//            {
//                color = "yellow";
//            }
//            else if (command.Item1 == "Tutorial" && Game.World != null && Game.World.GetState<TutorialHandler>().Visible)
//            {
//                color = "cyan";
//            }
//            //else if (!Game.Controls.MenuSelectable)
//            else if (!Game.Controls.IsMenuSelectable(command.Item1))
//            {
//                color = "gray";
//            }
//            //else if (command.Item1 == "Log" && Game.World.GetState<MessageHandler>().Unread)
//            //{
//            //    color = "orange";
//            //}
//            else if (command.Item1 == "Log" && Game.World.GetState<MessageHandler>().Unread)
//            {
//                color = Game.World.GetState<MessageHandler>().UnreadColor;
//            }
//            int adjust = (i == 0) ? -4 * CharWidth : 0;
//            var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
//            var bump = margin + adjust + text.Length * CharWidth + margin;
//            Game.Sprites.DrawString(Font, text, v, Game.Colors[color]);
//            total += bump;
//        }