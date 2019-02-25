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
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
        }

        public override void Act()
        {
            Actor a = Worker.GetComponent<Actor>();
            a.Patrol(X, Y, Z);
        }
    }
}
