using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    class ForbidTask : Task
    {
        public ForbidTask() : base()
        {
            _name = "forbid area";
            RequiresStructures = new List<Type>() { typeof(GuardPost) };
            _bg = "red";
        }

        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "ForbidTask", squares: squares);
            base.SelectZone(squares);
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

