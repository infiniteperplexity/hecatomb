using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    class PatrolTask : Task
    {
        public PatrolTask() : base()
        {
            MenuName = "patrol area";
            PrereqStructures = new List<string>() { "GuardPost" };
            BG = "#009999";
        }

        public override void ChooseFromMenu()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectTileControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.MenuSelectable = false;
            ControlContext.Set(c);
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "PatrolTask", x: c.X, y: c.Y, z: c.Z);
            base.SelectTile(c);
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override void Act()
        {
            Actor a = Worker.GetComponent<Actor>();
            a.Patrol(X, Y, Z);
        }
    }
}
