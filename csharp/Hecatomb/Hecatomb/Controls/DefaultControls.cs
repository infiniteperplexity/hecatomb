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
            KeyMap[Keys.Enter] = Commands.TogglePause;
			KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
			KeyMap[Keys.OemPipe] = Commands.ShowConsole;
            //KeyMap[Keys.A] = Commands.ShowAchievements;
            KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            KeyMap[Keys.PageUp] = Commands.ScrollUpCommand;
            KeyMap[Keys.PageDown] = Commands.ScrollDownCommand;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            KeyMap[Keys.D0] = Commands.ChooseMenuZero;
            KeyMap[Keys.D1] = Commands.ChooseMenuOne;
            KeyMap[Keys.D2] = Commands.ChooseMenuTwo;
            KeyMap[Keys.D3] = Commands.ChooseMenuThree;
            KeyMap[Keys.D4] = Commands.ChooseMenuFour;
            KeyMap[Keys.D5] = Commands.ChooseMenuFive;
            KeyMap[Keys.D6] = Commands.ChooseMenuSix;
            KeyMap[Keys.OemQuestion] = Commands.ChooseMenuSeven;


            MenuTop = new List<ColoredText>() {
                "Esc: System view.",
                "{yellow}Avatar mode (Tab: Camera mode)",
                " ",
                "Move: NumPad/Arrows, ,/.: Up/Down.",
                "(Control+Arrows for diagonal.)",
                "Wait: NumPad 5 / Space.",
                " ",
                "Enter: Enable auto-pause.",
                "+/-: Change speed.",
                " ",
                "Z: Cast spell, J: Assign job.",
                "M: Minions, S: Structures, U: Summary.",
                "G: Pick Up, D: Drop.",
                "I: Inventory, E: Equip/Unequip.",
                " ",
                "PageUp/Down: Scroll messages.",
                "A: Achievements, /: Toggle tutorial."
             //"0:Game 1:Camera 2:Commands 3:Log 4:Spells 5:Jobs 6:Achievements 7:Structures 8:Minions 9:Research"
			};
		}

        /* This will want...
 * 1) Camera / avatar mode.
 * 2) Spells.
 * 3) Jobs.
 * 4) Help / Key shortcuts
 * 5) Toggle tutorial.
 * 6) Message log.
 * 7) Overview.
 * 8) Hover.
 * 9) Pause and speed.
 * 10) Achievements.
 * 11) Tech tree.
 * 12) Structures.
 * 13) Minions.
 * 14) System view.
 */

        public override void ClickTile(Coord c)
		{
            var (x, y, z) = c;
            Creature cr = Game.World.Creatures[x, y, z];
            bool visible = Game.Visible.Contains(c);
            if (cr!=null && visible)
            {
                Game.Controls.Set(new MenuChoiceControls(cr));
                return;
            }
            Feature fr = Game.World.Features[x, y, z];
            if (fr?.TryComponent<StructuralComponent>()!=null && fr.GetComponent<StructuralComponent>().Structure.Placed)
            {
                Game.Controls.Set(new MenuChoiceControls(fr.GetComponent<StructuralComponent>().Structure.Unbox()));
            }
            else if (fr?.TryComponent<DoorFeatureComponent>()!=null)
            {
                //Game.Controls.Set(new MenuChoiceControls(fr.GetComponent<DoorFeatureComponent>()));
            }
        }
	}
}
