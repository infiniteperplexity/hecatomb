using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    public class CameraControls : AbstractCameraControls
    {
        public CameraControls() : base()
        {
            var Commands = InterfaceState.Commands!;
            //KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            keyMap[Keys.Tab] = Commands.ToggleMovingCamera;
            //KeyMap[Keys.J] = Commands.ChooseTask;
            keyMap[Keys.Z] = Commands.ChooseSpell;
            keyMap[Keys.J] = Commands.ChooseTask;
            //KeyMap[Keys.Z] = Commands.ChooseSpell;
            //KeyMap[Keys.L] = Commands.ShowLog;
            //KeyMap[Keys.V] = Commands.ShowAchievements;
            //KeyMap[Keys.R] = Commands.ShowResearch;
            //KeyMap[Keys.U] = Commands.ShowStructures;
            //KeyMap[Keys.M] = Commands.ShowMinions;

            //KeyMap[Keys.Enter] = Commands.TogglePause;
            //KeyMap[Keys.OemPipe] = Commands.ShowConsole;
            //KeyMap[Keys.OemQuestion] = Commands.ToggleTutorial;
            //KeyMap[Keys.PageUp] = Commands.ScrollUpCommand;
            //KeyMap[Keys.PageDown] = Commands.ScrollDownCommand;
            //KeyMap[Keys.OemMinus] = Commands.SlowDown;
            //KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            //KeyMap[Keys.Space] = SelectOrWait;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            MenuTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Navigate (Tab: Avatar)",
                " "
             };
        }
    }
}
