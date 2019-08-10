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
			Camera c = Game.Camera;
			Z = c.Z;
			XOffset = c.XOffset;
			YOffset = c.YOffset;
		}
		
		
		public CameraControls() : base()
		{
			var Commands = Game.Commands;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.Up] = Commands.MoveCameraNorth;
			KeyMap[Keys.Down] = Commands.MoveCameraSouth;
			KeyMap[Keys.Left] = Commands.MoveCameraWest;
			KeyMap[Keys.Right] = Commands.MoveCameraEast;
            KeyMap[Keys.W] = Commands.MoveCameraNorth;
            KeyMap[Keys.S] = Commands.MoveCameraSouth;
            KeyMap[Keys.A] = Commands.MoveCameraWest;
            KeyMap[Keys.D] = Commands.MoveCameraEast;
            KeyMap[Keys.Q] = Commands.MoveCameraNorthWest;
            KeyMap[Keys.E] = Commands.MoveCameraNorthEast;
            KeyMap[Keys.X] = Commands.MoveCameraSouthWest;
            KeyMap[Keys.C] = Commands.MoveCameraSouthEast;

            KeyMap[Keys.OemComma] = Commands.MoveCameraUp;
			KeyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
			KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            // skip for subclasses
            if (GetType()==typeof(CameraControls))
			{
				KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
                KeyMap[Keys.J] = Commands.ChooseTask;
                KeyMap[Keys.Z] = Commands.ChooseSpell;
            }
			
			MenuTop = new List<ColoredText>() {
				"Esc: System view.",
				"{yellow}Camera mode (Tab: Avatar mode)",
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
			    " ",
			    "PageUp/Down: Scroll messages.",
			    "A: Achievements, /: Toggle tutorial."
			};
		}

        public override void CameraHover()
        {
            if (Cursor.X > -1)
            {
                Coord tile = new Coord(Cursor.X, Cursor.Y, Game.Camera.Z);
                OnTileHover(tile);
            }
        }
    }
}
