using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
    public class MenuCameraControls : MenuChoiceControls
    {
        
        // this guy doesn't actually inherit all that much functionality from MenuChoiceControls...but we'll leave it as-is for now
        public MenuCameraControls(IChoiceMenu chooser) : base(chooser)
        {
        }

        public override void RefreshContent()
        {
            Chooser.BuildMenu(this);
            var Commands = OldGame.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] = Reset;
            MenuTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                " ",
                ("{yellow}"+Header)
            };
            // I think generally we want to avoid having alphabet-keyed choices here.
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
            KeyMap[Keys.Space] = SelectOrWait;

            for (int i = 0; i < Choices.Count; i++)
            {
                KeyMap[Alphabet[i]] = Choices[i].ChooseFromMenu;
                ColoredText ct = new ColoredText(alphabet[i] + ") ") + Choices[i].ListOnMenu();
                MenuTop.Add(ct);
            }
            Chooser.FinishMenu(this);
        }

        public void SelectOrWait()
        {
            if (OldGame.ReconstructMode)
            {
                OldGame.World.GetState<CommandLogger>().StepForward();
            }
            else if (ControlDown)
            {
                OldGame.Commands.Wait();
            }
            else
            {
                //var m = Mouse.GetState();
                //HandleClick(m.X, m.Y);
                SelectTile();
                // unless we selected something, wait anyway
                //if (Game.Controls is MenuCameraControls)
                if (OldGame.Controls == this)
                {
                    OldGame.Commands.Wait();
                }
            }
        }

        public override void CameraHover()
        {
            if (Cursor.X > -1)
            {
                Coord tile = new Coord(Cursor.X, Cursor.Y, OldGame.Camera.Z);
                OnTileHover(tile);
            }
        }

        public override void HandleKeyDown(Keys key)
        {
            base.HandleKeyDown(key);
            Camera c = OldGame.Camera;
            AbstractCameraControls.Z = c.Z;
            AbstractCameraControls.XOffset = c.XOffset;
            AbstractCameraControls.YOffset = c.YOffset;
        }

    }
}

