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
    class ForbidTask : Task
    {
        public ForbidTask() : base()
        {
            MenuName = "forbid area";
            PrereqStructures = new List<string>() { "GuardPost" };
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override bool CanAssign(Creature c)
        {
            return false;
        }
    }
}

