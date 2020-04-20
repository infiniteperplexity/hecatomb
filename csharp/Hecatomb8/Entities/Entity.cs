using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    
    public class Entity
    {
        static int MaxEID = -1;
        public int? EID;
        public string ClassName;
        [JsonIgnore] Dictionary<Type, Func<GameEvent, GameEvent>> Listeners;

        //public bool Exists;

        protected Entity()
        {
            ClassName = this.GetType().Name;
            Listeners = new Dictionary<Type, Func<GameEvent, GameEvent>>();
        }

        public bool Spawned
        {
            get => !(EID is null);
        }

        // until an Entity is "Spawned", it doesn't really exist in the game world
        public static T Spawn<T>() where T: Entity, new()
        {
            T t = new T();
            int n = MaxEID + 1;
            t.EID = n;
            MaxEID = n;
            GameState.World!.Entities[n] = t;
            if (t is ComposedEntity)
            {
                var ce = (ComposedEntity) (Object) t;
                ce.SpawnComponents();
            }
            return t;
        }

        // take an existing Entity that doesn't yet "exist" in the world, and spawn it
        public static T Spawn<T>(T t) where T : Entity
        {
            int n = MaxEID + 1;
            t.EID = n;
            MaxEID = n;
            GameState.World!.Entities[n] = t;
            if (t is ComposedEntity)
            {
                var ce = (ComposedEntity)(Object)t;
                ce.SpawnComponents();
            }
            return t;
        }


        public static Entity Spawn(Type type)
        {
            var maybe = Activator.CreateInstance(type);
            if (maybe == null)
            {
                throw new InvalidOperationException($"Couldn't spawn entity of type {type}, is usually caused by a problem in the constructor.");
            }
            var t = (Entity)maybe;
            int n = MaxEID + 1;
            t!.EID = n;
            MaxEID = n;
            GameState.World!.Entities[n] = t;
            if (t is ComposedEntity)
            {
                var ce = (ComposedEntity)(Object)t;
                ce.SpawnComponents();
            }
            return t;
        }

        public static T Spawn<T>(Type t) where T : Entity
        {
            return (T)Spawn(t);
        }
    }
}
