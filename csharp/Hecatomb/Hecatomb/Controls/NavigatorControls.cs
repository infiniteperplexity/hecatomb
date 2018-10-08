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
	public class NavigatorControls : ControlContext
	{
		static int Z;
		static int XOffset;
		static int YOffset;
		
		public override void HandleKeyDown(Keys key)
		{
			base.HandleKeyDown(key);
			GameCamera c = Game.Camera;
			Z = c.z;
			XOffset = c.XOffset;
			YOffset = c.YOffset;
		}
		
		
		public NavigatorControls() : base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Up] = Commands.MoveCameraNorth;
			KeyMap[Keys.Down] = Commands.MoveCameraSouth;
			KeyMap[Keys.Left] = Commands.MoveCameraWest;
			KeyMap[Keys.Right] = Commands.MoveCameraEast;
			KeyMap[Keys.OemComma] = Commands.MoveCameraUp;
			KeyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
			KeyMap[Keys.Space] = Commands.Wait;
			
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
	}
}
