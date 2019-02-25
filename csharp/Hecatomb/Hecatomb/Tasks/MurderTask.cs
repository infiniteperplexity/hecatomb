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
    class MurderTask : Task
    {
        public MurderTask() : base()
        {
            MenuName = "declare hostile";
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
        }
    }
}
