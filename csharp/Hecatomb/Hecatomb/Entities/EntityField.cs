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
    // abstract base class helps check type membership
    public abstract class EntityFieldBase { public int EID = -1; }
    public class EntityField<T> : EntityFieldBase where T : Entity
    {
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

        [JsonIgnore]
        public string ClassName
        {
            get
            {
                return Entity.ClassName;
            }
        }

        

        public void Despawn()
        {
            Entity.Despawn();
            EID = -1;
        }


        public static implicit operator T(EntityField<T> et)
        {
            return et.Entity;
        }

        public static implicit operator int(EntityField<T> et)
        {
            if (object.ReferenceEquals(et, null))
            {
                return -1;
            }
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
            if (object.ReferenceEquals(obj, null))
            {
                return (EID == -1);
            }
            // problem...what if there is a subtype relationship with T?
            else if (obj is int)
            {
                return (EID == (int)obj);
            }
            else if (obj is Entity)
            {
                return Entity == (Entity)Entity;
            }
            else if (obj is EntityFieldBase)
            {
                return (EID == ((EntityFieldBase)obj).EID);
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(EntityField<T> one, EntityField<T> two)
        {
            if (object.ReferenceEquals(one, null))
            {
                if (object.ReferenceEquals(two, null))
                {
                    return true;
                }
                return false;
            }
            return one.Equals(two);
        }

        public static bool operator !=(EntityField<T> one, EntityField<T> two)
        {
            if (object.ReferenceEquals(one, null))
            {
                if (object.ReferenceEquals(two, null))
                {
                    return false;
                }
                return true;
            }
            return !one.Equals(two);
        }

        public override int GetHashCode()
        {
            return EID.GetHashCode() + Entity.GetHashCode();
        }

        public T Unbox()
        {
            return Entity;
        }
    }
}
