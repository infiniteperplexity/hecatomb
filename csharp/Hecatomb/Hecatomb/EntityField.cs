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
    class EntityField<T> where T : GameEntity
    {
        public int EID;
        private T Entity
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

        public EntityField(int eid)
        {
            EID = eid;
        }

        public static implicit operator T(EntityField<T> et)
        {
            return et.Entity;
        }
    }

    public class EntityFieldConverter<T> : JsonConverter where T: GameEntity
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(EntityField<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new EntityField<T>(JToken.Load(reader).ToObject<int>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            EntityField<T> ef = (EntityField<T>)value;
            JToken.FromObject(ef.EID).WriteTo(writer);
        }
    }

}
