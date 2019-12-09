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
    public abstract class AbstractCameraControls : ControlContext
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


        public AbstractCameraControls() : base()
        {
            var Commands = Game.Commands;
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
