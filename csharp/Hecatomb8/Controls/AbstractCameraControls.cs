using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    // Note that if I update this functionality, I should consider updating MenuCameraControls as well
    public abstract class AbstractCameraControls : ControlContext
    {
        public static int Z;
        public static int XOffset;
        public static int YOffset;

        public override void HandleKeyDown(Keys key)
        {
            base.HandleKeyDown(key);
            Camera c = InterfaceState.Camera!;
            Z = c.Z;
            XOffset = c.XOffset;
            YOffset = c.YOffset;
        }


        public AbstractCameraControls() : base()
        {
            var Commands = InterfaceState.Commands!;
            keyMap[Keys.Up] = Commands.MoveCameraNorth;
            keyMap[Keys.Down] = Commands.MoveCameraSouth;
            keyMap[Keys.Left] = Commands.MoveCameraWest;
            keyMap[Keys.Right] = Commands.MoveCameraEast;
            keyMap[Keys.W] = Commands.MoveCameraNorth;
            keyMap[Keys.S] = Commands.MoveCameraSouth;
            keyMap[Keys.A] = Commands.MoveCameraWest;
            keyMap[Keys.D] = Commands.MoveCameraEast;
            keyMap[Keys.Q] = Commands.MoveCameraNorthWest;
            keyMap[Keys.E] = Commands.MoveCameraNorthEast;
            keyMap[Keys.X] = Commands.MoveCameraSouthWest;
            keyMap[Keys.C] = Commands.MoveCameraSouthEast;
            keyMap[Keys.OemComma] = Commands.MoveCameraUp;
            keyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
        }
        public override void CameraHover()
        {
            //if (Cursor.X > -1)
            //{
            //    Coord tile = new Coord(Cursor.X, Cursor.Y, Game.Camera.Z);
            //    OnTileHover(tile);
            //}
        }
    }
}
