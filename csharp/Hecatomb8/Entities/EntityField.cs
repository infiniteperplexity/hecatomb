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
    //using static HecatombAliases;
    public class EntityField<T> where T : Entity
    {
        public int? EID;
        // "briefly" because it's safest to 
        public T? UnboxBriefly()
        {
            if (EID == null)
            {
                return null;
            }
            int eid = (int)EID;
            if (!GameState.World!.Entities.ContainsKey(eid))
            {
                // do I actually ever rely on this for cleanup? if it works it's potentially very useful
                EID = null;
                return null;
            }
            return (T)GameState.World!.Entities[eid];
        }

        public static implicit operator EntityField<T>(T t)
        {
            return new EntityField<T>() { EID = t.EID };
        }

        public static implicit operator EntityField<T>(int eid)
        {
            return new EntityField<T>() { EID = eid };
        }
    }
}
