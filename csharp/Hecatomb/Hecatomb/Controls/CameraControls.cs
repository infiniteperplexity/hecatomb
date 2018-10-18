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
	public class CameraControls : ControlContext
	{
		static int Z;
		static int XOffset;
		static int YOffset;
		
		public override void HandleKeyDown(Keys key)
		{
			base.HandleKeyDown(key);
			GameCamera c = Game.Camera;
			Z = c.Z;
			XOffset = c.XOffset;
			YOffset = c.YOffset;
		}
		
		
		public CameraControls() : base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Up] = Commands.MoveCameraNorth;
			KeyMap[Keys.Down] = Commands.MoveCameraSouth;
			KeyMap[Keys.Left] = Commands.MoveCameraWest;
			KeyMap[Keys.Right] = Commands.MoveCameraEast;
			KeyMap[Keys.OemComma] = Commands.MoveCameraUp;
			KeyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
			KeyMap[Keys.Space] = Commands.Wait;
			// skip for subclasses
			if (GetType()==typeof(CameraControls))
			{
				KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
			}
			
			MenuTop = new List<string>() {
				"Esc: System view.",
				"Camera mode (Tab: Avatar mode)",
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
			TopColors = new TextColors(1, "yellow");
		}
	}
}
