using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    public class DoorFeatureComponent : Component, IChoiceMenu
    {
        public void BuildMenu(MenuChoiceControls menu)
        {
            // *** Do we still use this?
            menu.Header = "Door";
            menu.Choices = new List<IMenuListable>();
            //Entity.Unbox().Highlight = "lime green";
        }

        public void FinishMenu(MenuChoiceControls menu)
        {
            Feature f = (Feature)Entity;
            // this should...unlock?  so "solid" isn't the right thing...should it forbid?
            if (f.Solid)
            {
                menu.MenuTop.Add("L) Lock door.");
            }
            else
            {
                menu.MenuTop.Add("L) Unlock door.");
            }
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.L] = () => { };
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.Escape] =
                () =>
                {
                    ControlContext.Selection = null;
                    Game.Controls.Reset();
                };
        }

    }
}
