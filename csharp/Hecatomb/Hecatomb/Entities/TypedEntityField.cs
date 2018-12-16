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

    public class TypedEntityField<T> : TileEntityField<T> where T : TypedEntity
    {
        [JsonIgnore] Dictionary<string, EntityField<Component>> Components
        {
            get
            {
                return Entity.Components;
            }
        }
        public S GetComponent<S>() where S : Component
        {
            return Entity.GetComponent<S>();
        }
        public S TryComponent<S>() where S : Component
        {
            return Entity.TryComponent<S>();
        }

        public static implicit operator TypedEntityField<T>(T t)
        {
            return new TypedEntityField<T>() { EID = t.EID };
        }

        public static implicit operator TypedEntityField<T>(int eid)
        {
            return new TypedEntityField<T>() { EID = eid };
        }
    }
}
