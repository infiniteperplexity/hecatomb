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
    public class ListenerHandledEntityHandle<T> where T : Entity
    {    
        // this is what gets serialized
        public int _eid;



        private ListenerHandledEntityHandle()
        {
            // only ever use this for serialization
        }

        private ListenerHandledEntityHandle(T t)
        {
            _eid = (int)t.EID!;
        }

        public static ListenerHandledEntityHandle<T> CreateHandle(T t)
        {
            return new ListenerHandledEntityHandle<T>(t);
        }
        public T? UnboxBriefly()
        {
            if (!Entities.ContainsKey(_eid))
            {
                return null;
            }
            else
            {
                return (T)Entities[_eid];
            }
        }

        public bool Is(T? t)
        {
            if (t is null)
            {
                return false;
            }
            else
            {
                return (t.EID == _eid && Entities.ContainsKey(_eid));
            }
        }
    }
}
