using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Entity
    {
        public static int MaxEID = -1;
        public int? EID;
        public string Class;
        [JsonIgnore] Dictionary<Type, Func<GameEvent, GameEvent>> Listeners;

        //public bool Exists;

        protected Entity()
        {
            Class = this.GetType().Name;
            Listeners = new Dictionary<Type, Func<GameEvent, GameEvent>>();
        }

        [JsonIgnore] public bool Spawned
        {
            get => !(EID is null);
        }


        public static T Mock<T>() where T: Entity, new()
        {
            T t = new T();
            return t;
        }


        public static Entity Mock(Type type)
        {
            var maybe = Activator.CreateInstance(type);
            if (maybe == null)
            {
                throw new InvalidOperationException($"Couldn't spawn entity of type {type}, is usually caused by a problem in the constructor.");
            }
            var t = (Entity)maybe;
            return t;
        }
        // until an Entity is "Spawned", it doesn't really exist in the game world
        public static T Spawn<T>() where T : Entity, new()
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
            foreach (Type type in t.Listeners.Keys)
            {
                Subscribe(type, t, t.Listeners[type]);
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
            foreach (Type type in t.Listeners.Keys)
            {
                Subscribe(type, t, t.Listeners[type]);
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
            foreach (Type lt in t.Listeners.Keys)
            {
                Subscribe(lt, t, t.Listeners[lt]);
            }
            return t;
        }

        public static T Spawn<T>(Type t) where T : Entity
        {
            return (T)Spawn(t);
        }

        public virtual void Despawn()
        {
            if (EID != null)
            {
                Publish(new DespawnEvent() { Entity = this });
                GameState.World!.Events.UnsubscribeAll(this);
                if (GameState.World!.Entities.ContainsKey((int)EID))
                {
                    GameState.World!.Entities.Remove((int)EID);
                }
            }
            EID = null;
        }

        public void AddListener<T>(Func<GameEvent, GameEvent> f)
        {
            Listeners[typeof(T)] = f;
        }
        // request a pointer to this entity, by submitting a listener method you're promising handles this entity despawning
        public ListenerHandledEntityHandle<T> GetHandle<T>(Func<GameEvent, GameEvent> ge) where T : Entity
        {
              return ListenerHandledEntityHandle<T>.CreateHandle((T)this);
        }

        public static T? GetEntity<T>(int? eid) where T: Entity
        {
            if (eid is null)
            {
                return null;
            }
            else if (!Entities.ContainsKey((int)eid))
            {
                return null;
            }
            else
            {
                return (T)Entities[(int)eid];
            }
        }

        public static bool Exists(int? eid)
        {
            if (eid is null)
            {
                return false;
            }
            else
            {
                return Entities.ContainsKey((int)eid);
            }
        }
    }
}
