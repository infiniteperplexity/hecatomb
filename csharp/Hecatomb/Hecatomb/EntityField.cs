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
    public struct EntityField<T> where T : GameEntity
    {
        public int EID;
        [JsonIgnore] public T Entity
        {
            get
            {
                if (!Game.World.Entities.Spawned.ContainsKey(EID))
                {
                    EID = -1;
                    return null;
                }
                return (T) Game.World.Entities.Spawned[EID];
            }
            set
            {
                // could check if not spawned...
                if (value == null)
                {
                    EID = -1;
                }
                else
                {
                    EID = value.EID;
                }
            }
        }

        public static implicit operator T(EntityField<T> et)
        {
            return et.Entity;
        }

        public static implicit operator int(EntityField<T> et)
        {
            return et.EID;
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
