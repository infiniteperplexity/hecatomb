/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of NavigationControlContext.
	/// </summary>
	public class CameraControls : AbstractCameraControls
	{
		public CameraControls() : base()
		{
			var Commands = OldGame.Commands;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
            KeyMap[Keys.J] = Commands.ChooseTask;
            KeyMap[Keys.Z] = Commands.ChooseSpell;
            KeyMap[Keys.J] = Commands.ChooseTask;
            KeyMap[Keys.Z] = Commands.ChooseSpell;
            KeyMap[Keys.L] = Commands.ShowLog;
            KeyMap[Keys.V] = Commands.ShowAchievements;
            KeyMap[Keys.R] = Commands.ShowResearch;
            KeyMap[Keys.U] = Commands.ShowStructures;
            KeyMap[Keys.M] = Commands.ShowMinions;

            KeyMap[Keys.Enter] = Commands.TogglePause;
            KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
            KeyMap[Keys.OemPipe] = Commands.ShowConsole;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.PageUp] = Commands.ScrollUpCommand;
            KeyMap[Keys.PageDown] = Commands.ScrollDownCommand;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            KeyMap[Keys.Space] = SelectOrWait;
            RefreshContent();
		}

        public void SelectOrWait()
        {
            if (ControlDown)
            {
                OldGame.Commands.Wait();
            }
            else
            {
                //var m = Mouse.GetState();
                //HandleClick(m.X, m.Y);
                SelectTile();
                // unless we selected something, wait anyway
                if (OldGame.Controls is CameraControls)
                {
                    OldGame.Commands.Wait();
                }
            }
        }

        public override void RefreshContent()
        {
             MenuTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Navigate (Tab: Avatar)",
                " "
             };
            if (OldGame.World != null && OldGame.World.Player != null)
            {
                var p = OldGame.World.Player;
                var time = OldGame.Time.GetTimeText();
                MenuTop.Add(time[0]);
                MenuTop.Add(time[1]);
                MenuTop.Add(" ");
                MenuTop.Add(p.GetComponent<SpellCaster>().GetSanityText());
                if (OldGame.World.GetState<TaskHandler>().Minions.Count > 0)
                {
                    MenuTop.Add(" ");
                    MenuTop.Add("Minions:");
                    var types = new Dictionary<string, int>();
                    foreach (var minion in OldGame.World.GetState<TaskHandler>().Minions)
                    {
                        Creature c = (Creature)minion;
                        if (!types.ContainsKey(c.TypeName))
                        {
                            types[c.TypeName] = 1;
                        }
                        else
                        {
                            types[c.TypeName] += 1;
                        }
                    }
                    foreach (var type in types.Keys)
                    {
                        var mock = Entity.Mock<Creature>(type);
                        // might need better handling for when we have multiple zombie types that still share a TypeName?
                        MenuTop.Add("{" + mock.FG + "}" + type + ": " + types[type]);
                    }
                }

                var stored = new List<Dictionary<string, int>>();
                var structures = Structure.ListStructures();
                foreach (Structure s in structures)
                {
                    stored.Add(s.GetStored());
                }
                var total = Item.CombinedResources(stored);
                if (total.Count > 0)
                {
                    MenuTop.Add(" ");
                    MenuTop.Add("Stored resources:");
                    foreach (var res in total.Keys)
                    {
                        var r = Resource.Types[res];
                        MenuTop.Add("{" + Resource.GetListColor(res) + "} - " + Resource.Format((res, total[res])));
                    }
                }
                var messages = OldGame.World.GetState<MessageHandler>().MessageHistory;
                if (messages.Count > 0)
                {
                    MenuTop.Add(" ");
                    var txt = messages[0];
                    if (!txt.Colors.ContainsKey(0))
                    {
                        txt = new ColoredText("{cyan}" + txt.Text);
                    }
                    MenuTop.Add(txt);
                }
            }
        }
    }
}
