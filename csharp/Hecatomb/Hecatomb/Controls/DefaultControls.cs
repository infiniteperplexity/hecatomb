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
	class DefaultControls : ControlContext
	{
		public DefaultControls() : base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Up] = Commands.MoveNorthCommand;
			KeyMap[Keys.Down] = Commands.MoveSouthCommand;
			KeyMap[Keys.Left] = Commands.MoveWestCommand;
			KeyMap[Keys.Right] = Commands.MoveEastCommand;
			KeyMap[Keys.OemComma] = Commands.MoveUpCommand;
			KeyMap[Keys.OemPeriod] = Commands.MoveDownCommand;
			KeyMap[Keys.Space] = Commands.Wait;
			KeyMap[Keys.J] = Commands.ChooseTask;
			KeyMap[Keys.Z] = Commands.ChooseSpell;
			KeyMap[Keys.S] = Commands.SaveGameCommand;
			KeyMap[Keys.R] = Commands.RestoreGameCommand;
			
			MenuText = new List<string>() {
				"Esc: System view.",
				"Avatar mode (Tab: Navigation mode)",
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
			};
			TextColors = new Dictionary<Tuple<int, int>, string>() {
				{new Tuple<int, int>(1,0), "yellow"}
			};
		}
		
		public override void ClickTile(Coord c)
		{
		}
	}
}
