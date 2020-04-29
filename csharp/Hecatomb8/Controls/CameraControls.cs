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
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.L] = Commands.ShowLog;
            //KeyMap[Keys.V] = Commands.ShowAchievements;
            //KeyMap[Keys.R] = Commands.ShowResearch;
            KeyMap[Keys.U] = Commands.ShowStructures;
            KeyMap[Keys.M] = Commands.ShowMinions;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.Enter] = Commands.TogglePause;
            //KeyMap[Keys.OemPipe] = Commands.ShowConsole;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            KeyMap[Keys.Space] = SelectOrWait;
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
                //if (Game.World.GetState<TaskHandler>().Minions.Count > 0)
                //{
                //    MenuTop.Add(" ");
                //    MenuTop.Add("Minions:");
                //    var types = new Dictionary<string, int>();
                //    foreach (var minion in Game.World.GetState<TaskHandler>().Minions)
                //    {
                //        Creature c = (Creature)minion;
                //        if (!types.ContainsKey(c.TypeName))
                //        {
                //            types[c.TypeName] = 1;
                //        }
                //        else
                //        {
                //            types[c.TypeName] += 1;
                //        }
                //    }
                //    foreach (var type in types.Keys)
                //    {
                //        var mock = Entity.Mock<Creature>(type);
                //        // might need better handling for when we have multiple zombie types that still share a TypeName?
                //        MenuTop.Add("{" + mock.FG + "}" + type + ": " + types[type]);
                //    }
                //}

                //var stored = new List<Dictionary<string, int>>();
                //var structures = Structure.ListStructures();
                //foreach (Structure s in structures)
                //{
                //    stored.Add(s.GetStored());
                //}
                //var total = Item.CombinedResources(stored);
                //if (total.Count > 0)
                //{
                //    MenuTop.Add(" ");
                //    MenuTop.Add("Stored resources:");
                //    foreach (var res in total.Keys)
                //    {
                //        var r = Resource.Types[res];
                //        MenuTop.Add("{" + Resource.GetListColor(res) + "} - " + Resource.Format((res, total[res])));
                //    }
                //}

                //var messages = Game.World.GetState<MessageHandler>().MessageHistory;
                //if (messages.Count > 0)
                //{
                //    MenuTop.Add(" ");
                //    var txt = messages[0];
                //    if (!txt.Colors.ContainsKey(0))
                //    {
                //        txt = new ColoredText("{cyan}" + txt.Text);
                //    }
                //    MenuTop.Add(txt);
                //}
            }
        }
    }
}
