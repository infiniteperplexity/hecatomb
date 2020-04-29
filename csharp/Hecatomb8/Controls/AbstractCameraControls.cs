using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
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
            var c = InterfaceState.Cursor;
            if (c != null)
            {
                Coord tile = new Coord(((Coord)c)!.X, ((Coord)c)!.Y, InterfaceState.Camera!.Z);
                HoverTile(tile);
            }
        }

        public void SelectOrWait()
        {
            var Commands = InterfaceState.Commands!;
            if (ControlDown)
            {
                Commands.Wait();
            }
            else
            {
                SelectTile();
                // unless we selected something, wait anyway
                if (InterfaceState.Controls == this)
                {
                    Commands.Wait();
                }
            }
        }
    }
}
