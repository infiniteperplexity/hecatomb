using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    class DefaultControls : ControlContext
    {
        public DefaultControls()
        {
            var commands = InterfaceState.Commands;
            KeyMap = new Dictionary<Keys, Action>()
            {
                [Keys.Tab] = commands!.ToggleMovingCamera,
                [Keys.W] = commands!.MoveNorthCommand,
                [Keys.A] = commands!.MoveWestCommand,
                [Keys.S] = commands!.MoveSouthCommand,
                [Keys.D] = commands!.MoveEastCommand,
                [Keys.Up] = commands!.MoveNorthCommand,
                [Keys.Left] = commands!.MoveWestCommand,
                [Keys.Down] = commands!.MoveSouthCommand,
                [Keys.Right] = commands!.MoveEastCommand,
                [Keys.Space] = commands!.Wait,
                [Keys.J] = commands!.ChooseTask,
                [Keys.Z] = commands!.ChooseSpell,
                [Keys.Escape] = commands!.SystemMenuCommand,
                [Keys.T] = HecatombConverter.Test
        };
            MenuTop = new List<ColoredText>();
            RefreshContent();
        }

        public override void RefreshContent()
        {
            MenuTop = new List<ColoredText>() {
                "Esc: Game menu.",
                " ",
                "{yellow}Avatar (Tab: Navigate)",
                " "
            };
        }
    }
}
