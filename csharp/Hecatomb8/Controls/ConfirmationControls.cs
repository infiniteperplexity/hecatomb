using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public class ConfirmationControls : ControlContext
    {
        public string Header;
        public ConfirmationControls(ColoredText header, Action confirmed) : base()
        {
            AllowsUnpause = true;
            //MenuSelectable = false;
            Header = header;
            KeyMap[Keys.Escape] = GoBack;
            KeyMap[Keys.Y] = confirmed;
            KeyMap[Keys.N] = GoBack;
            RefreshContent();
        }

        public void GoBack()
        {
            if (GameState.World is null)
            {
                GameManager.BackToTitle();
            }
            else
            {
                InterfaceState.ResetControls();
            }
        }

        public override void RefreshContent()
        {
            InfoTop = new List<ColoredText>() {
                        "{orange}**" + Header + "**.",
                        "{orange}Y) Yes.",
                        "{orange}N) No."
                    };
            InterfaceState.DirtifyTextPanels();
        }

        public override void HandleClick(int x, int y)
        {

        }
        public override void HandleHover(int x, int y)
        {

        }
    }
}
