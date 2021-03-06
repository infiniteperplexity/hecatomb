﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    using static HecatombAliases;
    class DefaultControls : ControlContext
    {
        public DefaultControls()
        {
            MenuCommandsSelectable = true;
            var commands = InterfaceState.Commands!;
            KeyMap = new Dictionary<Keys, Action>()
            {
                [Keys.Tab] = commands!.ToggleMovingCamera,
                [Keys.W] = commands!.MoveNorthCommand,
                [Keys.A] = commands!.MoveWestCommand,
                [Keys.S] = commands!.MoveSouthCommand,
                [Keys.D] = commands!.MoveEastCommand,
                [Keys.E] = commands!.MoveNorthEastCommand,
                [Keys.Q] = commands!.MoveNorthWestCommand,
                [Keys.X] = commands!.MoveSouthWestCommand,
                [Keys.C] = commands!.MoveSouthEastCommand,
                [Keys.Up] = commands!.MoveNorthCommand,
                [Keys.Left] = commands!.MoveWestCommand,
                [Keys.Down] = commands!.MoveSouthCommand,
                [Keys.Right] = commands!.MoveEastCommand,
                [Keys.OemPeriod] = commands!.MoveDownCommand,
                [Keys.OemComma] = commands!.MoveUpCommand,
                [Keys.Space] = commands!.Wait,
                [Keys.NumPad5] = commands!.Wait,
                [Keys.NumPad2] = commands.MoveNorthCommand,
                [Keys.NumPad8] = commands.MoveSouthCommand,
                [Keys.NumPad4] = commands.MoveWestCommand,
                [Keys.NumPad6] = commands.MoveEastCommand,
                [Keys.NumPad1] = commands.MoveNorthEastCommand,
                [Keys.NumPad3] = commands.MoveNorthWestCommand,
                [Keys.NumPad7] = commands.MoveSouthWestCommand,
                [Keys.NumPad9] = commands.MoveSouthEastCommand,
                [Keys.J] = commands!.ChooseTask,
                [Keys.Z] = commands!.ChooseSpell,
                [Keys.U] = commands!.ShowStructures,
                [Keys.M] = commands!.ShowMinions,
                [Keys.L] = commands!.ShowLog,
                [Keys.V] = commands!.ShowAchievements,
                [Keys.R] = commands!.ShowResearch,
                [Keys.OemQuestion] = commands.ToggleTutorial,
                [Keys.Escape] = commands!.SystemMenuCommand,
                [Keys.Enter] = commands!.TogglePause,
                [Keys.OemMinus] = Commands.SlowDown,
                [Keys.OemPlus] = Commands.SpeedUp
        };
            InfoTop = new List<ColoredText>();
            RefreshContent();
        }

        public override void RefreshContent()
        {
            InfoTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Avatar (Tab: Navigate)",
                " "
            };
            if (GameState.World != null && Player != null)
            {
                var time = Time.GetTimeText();
                InfoTop.Add(time[0]);
                InfoTop.Add(time[1]);
                InfoTop.Add(" ");
                InfoTop.Add(Player.GetComponent<SpellCaster>().GetSanityText());
                if (GetState<TaskHandler>().Minions.Count > 0)
                {
                    InfoTop.Add(" ");
                    InfoTop.Add("Minions:");
                    var types = new Dictionary<Type, int>();
                    foreach (var minion in GetState<TaskHandler>().GetMinions())
                    {
                        Creature c = (Creature)minion;
                        if (!types.ContainsKey(c.GetType()))
                        {
                            types[c.GetType()] = 1;
                        }
                        else
                        {
                            types[c.GetType()] += 1;
                        }
                    }
                    foreach (var type in types.Keys)
                    {
                        // handle this manually so we're not constructing a million zombies a minute
                        var name = "Zombie";
                        var fg = "lime green";
                        if (type == typeof(Zombie))
                        {
                            name = "Zombie";
                            fg = "lime green";
                        }
                        // might need better handling for when we have multiple zombie types that still share a TypeName?
                        InfoTop.Add("{" + fg + "}" + name + ": " + types[type]);
                    }
                }

                var stored = new List<Dictionary<Resource, int>>();
                var structures = Structure.ListStructures();
                foreach (Structure s in structures)
                {
                    stored.Add(s.GetStored());
                }
                var total = Item.CombineResources(stored);
                if (total.Count > 0)
                {
                    InfoTop.Add(" ");
                    InfoTop.Add("Stored resources:");
                    foreach (var res in total.Keys)
                    {
                        InfoTop.Add("{" + res.TextColor + "} - " + Resource.Format((res, total[res])));
                    }
                }

                var messages = GetState<GameLog>().MessageHistory;
                if (messages.Count > 0)
                {
                    InfoTop.Add(" ");
                    var txt = messages[0];
                    if (!txt.Colors.ContainsKey(0))
                    {
                        txt = new ColoredText("{cyan}" + txt.Text);
                    }
                    InfoTop.Add(txt);
                }
            }
        }
    }
}
