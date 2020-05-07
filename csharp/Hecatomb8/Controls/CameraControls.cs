using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class CameraControls : AbstractCameraControls
    {
        public CameraControls() : base()
        {
            MenuCommandsSelectable = true;
            var Commands = InterfaceState.Commands!;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
            KeyMap[Keys.Z] = Commands.ChooseSpell;
            KeyMap[Keys.J] = Commands.ChooseTask;
            KeyMap[Keys.R] = Commands.ShowResearch;
            KeyMap[Keys.L] = Commands.ShowLog;
            KeyMap[Keys.V] = Commands.ShowAchievements;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.U] = Commands.ShowStructures;
            KeyMap[Keys.M] = Commands.ShowMinions;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.Enter] = Commands.TogglePause;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;
            KeyMap[Keys.Space] = SelectOrWait;
            KeyMap[Keys.NumPad5] = Commands.Wait;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            InfoTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Navigate (Tab: Avatar)",
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
