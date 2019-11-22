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
    /// Description of DefaultControlContext.
    /// </summary>
    /// 
    
	class DefaultControls : ControlContext
	{   
        
        
		public DefaultControls() : base()
		{
            // probably want to change how clicking works on panels.
            
            var Commands = Game.Commands;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
			KeyMap[Keys.Up] = Commands.MoveNorthCommand;
			KeyMap[Keys.Down] = Commands.MoveSouthCommand;
			KeyMap[Keys.Left] = Commands.MoveWestCommand;
			KeyMap[Keys.Right] = Commands.MoveEastCommand;
            KeyMap[Keys.W] = Commands.MoveNorthCommand;
            KeyMap[Keys.S] = Commands.MoveSouthCommand;
            KeyMap[Keys.A] = Commands.MoveWestCommand;
            KeyMap[Keys.D] = Commands.MoveEastCommand;
            KeyMap[Keys.E] = Commands.MoveNorthEastCommand;
            KeyMap[Keys.Q] = Commands.MoveNorthWestCommand;
            KeyMap[Keys.X] = Commands.MoveSouthWestCommand;
            KeyMap[Keys.C] = Commands.MoveSouthEastCommand;
            KeyMap[Keys.OemComma] = Commands.MoveUpCommand;
			KeyMap[Keys.OemPeriod] = Commands.MoveDownCommand;
			KeyMap[Keys.Space] = Commands.Wait;
			KeyMap[Keys.J] = Commands.ChooseTask;
			KeyMap[Keys.Z] = Commands.ChooseSpell;
            KeyMap[Keys.L] = Commands.ShowLog;
            KeyMap[Keys.V] = Commands.ShowAchievements;
            KeyMap[Keys.R] = Commands.ShowResearch;
            KeyMap[Keys.LeftControl] = Commands.HoverCamera;
            KeyMap[Keys.RightControl] = Commands.HoverCamera;

            KeyMap[Keys.Enter] = Commands.TogglePause;
			KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
			KeyMap[Keys.OemPipe] = Commands.ShowConsole;
            //KeyMap[Keys.A] = Commands.ShowAchievements;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.PageUp] = Commands.ScrollUpCommand;
            KeyMap[Keys.PageDown] = Commands.ScrollDownCommand;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;


            RefreshContent();
		}

        public override void RefreshContent()
        {
            MenuTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Avatar (Tab: Camera)",
                " "
			};
            if (Game.World != null && Game.World.Player != null)
            {
                var p = Game.World.Player;
                
                TurnHandler t = Game.World.Turns;
                if (Game.Time.AutoPausing || Game.Time.PausedAfterLoad)
                {
                    MenuTop.Add("{yellow}Paused");
                }
                string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
                //string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
                MenuTop.Add(time);
                MenuTop.Add(" ");
                MenuTop.Add($"Sanity: {p.GetComponent<SpellCaster>().Sanity}/{p.GetComponent<SpellCaster>().GetCalculatedMaxSanity()}");
                if (Game.World.GetState<TaskHandler>().Minions.Count > 0)
                {
                    MenuTop.Add($"Controls {Game.World.GetState<TaskHandler>().Minions.Count} minions.");
                }

                string paused = (Game.Time.PausedAfterLoad || Game.Time.AutoPausing) ? "{yellow}Paused" : "      ";
                
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
                        MenuTop.Add("- " + Resource.Format((res, total[res])));
                    }
                }
            }
        }
	}
}
