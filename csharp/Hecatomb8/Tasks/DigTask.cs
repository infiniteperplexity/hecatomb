using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class DigTask : Task
    {
        public DigTask()
        {
            MenuDescription = "dig or whatever";
            _bg = "orange";
        }

        public override void ChooseFromMenu()
        {
            //Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseDigTask" });
            //var c = new SelectTileControls(this);
            var c = new SelectZoneControls(this);
            //c.MenuSelectable = false;
            //c.SelectedMenuCommand = "Jobs";
            InterfaceState.SetControls(c);
        }
    }
}
