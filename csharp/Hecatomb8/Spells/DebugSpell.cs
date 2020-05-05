using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    
    class DebugSpell : Spell, IDisplayInfo
    {
        int? ChoiceIndex;

        public DebugSpell()
        {
            _cost = 0;
        }
        public override void ChooseFromMenu()
        {
            if (ChoiceIndex is null)
            {
                var c = new InfoDisplayControls(this);
                c.MenuCommandsSelectable = false;
                InterfaceState.SetControls(c);
            }
        }

        public void BuildInfoDisplay(InfoDisplayControls info)
        {

        }

        public void FinishInfoDisplay(InfoDisplayControls info)
        {

        }
    }
}
