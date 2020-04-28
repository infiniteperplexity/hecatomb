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
    using static HecatombAliases;
    // this clas is special and that's why it has a really long name
    // 1) the constructor is private; it shoudl only get populated by either calling CreatePointerFromOwnEntity from the pointed-to Entity, or during Deserialization
    // 2) in order to create it, you have to submit a Func<GameEvent, GameEvent>, which should handle despawning and potentially other events
    public class ListenerHandledEntityPointer<T> where T : Entity
    {    
        // this is what gets serialized
        public int _eid;
        [JsonIgnore] private T Entity;

        private ListenerHandledEntityPointer(T t)
        {
            Entity = t;
            _eid = (int)t.EID!;
        }

        public static ListenerHandledEntityPointer<T> CreatePointerFromOwnEntity(T t)
        {
            return new ListenerHandledEntityPointer<T>(t);
        }
        public T UnboxBriefly()
        {
            return Entity;
        }
        public void SetEntityDuringDeserialization(T t)
        {
            Entity = t;
        }
    }
}
