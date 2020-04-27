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
    // An EntityField is a wrapper for an Entity.  It's there largely to avoid circular references while serializing

    public class EntityField<T> where T : Entity
    {
        public int? EID;
        // I have named this "UnboxBriefly" because if you only ever use it immediately after unboxing, you are protected from errors caused by despawning
        public T? UnboxIfNotNull()
        {
            if (EID == null)
            {
                return null;
            }
            int eid = (int)EID;
            if (!GameState.World!.Entities.ContainsKey(eid))
            {
                return null;
            }
            return (T)GameState.World!.Entities[eid];
        }

        public EntityField<T> UpdateNullity()
        {
            if (EID is null)
            {
                return this;
            }
            int eid = (int)EID;
            if (!GameState.World!.Entities.ContainsKey(eid))
            {
                EID = null;
            }
            return this;
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
