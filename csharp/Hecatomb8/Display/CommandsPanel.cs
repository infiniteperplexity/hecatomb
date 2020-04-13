﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace Hecatomb
{
    public class CommandsPanel : InterfacePanel
    {
        public CommandsPanel(int x, int y, int w, int h) : base(x, y, w, h)
        {

            LeftMargin = 2;
            RightMargin = 2;
        }

        public override void Draw()
        {
            OldGame.Sprites.Draw(BG, new Vector2(X0, Y0), Color.Black);
            int total = 0;
            int margin = 4 * CharWidth; 
            for (int i = 0; i < OldGame.Controls.MenuCommands.Count; i++)
            {
                if (OldGame.World != null && OldGame.World.GetState<TutorialHandler>().Visible)
                {
                    var text = OldGame.World.GetState<TutorialHandler>().Current.MenuCommands[i];
                    var color = (text.Colors.ContainsKey(0)) ? text.Colors[0] : "white";
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    OldGame.Sprites.DrawString(Font, text, v, OldGame.Colors[color]);
                    total += bump;
                }
                else
                {
                    var command = OldGame.Controls.MenuCommands[i];
                    var text = command.Item2;
                    var color = "white";
                    //if (command.Item1 == Game.Controls.SelectedMenuCommand)
                    if (OldGame.Controls.IsMenuSelected(command.Item1))   
                    {
                        color = "yellow";
                    }
                    else if (command.Item1 == "Tutorial" && OldGame.World != null && OldGame.World.GetState<TutorialHandler>().Visible)
                    {
                        color = "cyan";
                    }
                    //else if (!Game.Controls.MenuSelectable)
                    else if (!OldGame.Controls.IsMenuSelectable(command.Item1))
                    {
                        color = "gray";
                    }
                    //else if (command.Item1 == "Log" && Game.World.GetState<MessageHandler>().Unread)
                    //{
                    //    color = "orange";
                    //}
                    else if (command.Item1 == "Log" && OldGame.World.GetState<MessageHandler>().Unread)
                    {
                        color = OldGame.World.GetState<MessageHandler>().UnreadColor;
                    }
                    int adjust = (i == 0) ? -4 * CharWidth : 0;
                    var v = new Vector2(X0 + total + margin + adjust, Y0 + TopMargin);
                    var bump = margin + adjust + text.Length * CharWidth + margin;
                    OldGame.Sprites.DrawString(Font, text, v, OldGame.Colors[color]);
                    total += bump;
                }
            }
        }
    }
}
