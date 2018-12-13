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
    using static HecatombAliases;
    public struct EntityField<T> where T : Entity
    {
        public int EID;
        [JsonIgnore] public T Entity
        {
            get
            {
                if (!Entities.ContainsKey(EID))
                {
                    EID = -1;
                    return null;
                }
                return (T)Entities[EID];
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
        [JsonIgnore] public int X
        {
            get
            {
                return (Entity as PositionedEntity).X;
            }
        }
        [JsonIgnore]
        public int Y
        {
            get
            {
                return (Entity as PositionedEntity).Y;
            }
        }
        [JsonIgnore]
        public int Z
        {
            get
            {
                return (Entity as PositionedEntity).Z;
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

        public override bool Equals(Object obj)
        {
            return (obj is EntityField<T> && this == (EntityField<T>)obj)
                || (obj is int && EID == (int)obj)
                || (obj is T && Entity == (T)Entity);
        }

        public static bool operator ==(EntityField<T> one, EntityField<T> two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(EntityField<T> one, EntityField<T> two)
        {
            return !one.Equals(two);
        }

        public override int GetHashCode()
        {
            return EID.GetHashCode() + Entity.GetHashCode();
        }
    }
}
