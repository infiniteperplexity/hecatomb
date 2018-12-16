using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class TaskField: TileEntityField<Task>
    {
        public void Act()
        {
            Entity.Act();
        }

        public static implicit operator TaskField(Task t)
        {
            return new TaskField() { EID = t.EID };
        }

        public static implicit operator TaskField(int eid)
        {
            return new TaskField() { EID = eid };
        }
    }
}
