/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 9:42 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
	public abstract class Entity
	{
        [JsonIgnore] public static int MaxEID;
        [JsonIgnore]public static Dictionary<int, Entity> Entities;
		public int EID;
		public string ClassName;
		[JsonIgnore] public bool Spawned;
        [JsonIgnore] public Dictionary<Type, Func<GameEvent, GameEvent>> Listeners;
	
        static Entity()
        {
            Reset();
        }

        public static void Reset()
        {
            MaxEID = -1;
            Entities = new Dictionary<int, Entity>();
        }
		protected Entity()
		{
            Listeners = new Dictionary<Type, Func<GameEvent, GameEvent>>();
			ClassName = this.GetType().Name;
			EID = -1;
			Spawned = false;
		}

        public void AddListener<T>(Func<GameEvent, GameEvent> f)
        {
            Listeners[typeof(T)] = f;
        }
			
		public void Publish(GameEvent g)
		{
				
		}

        public static Entity Spawn(Type t)
        {
            Entity ge = (Entity)Activator.CreateInstance(t);
            ge.EID = MaxEID + 1;
            MaxEID = ge.EID;
            Entities[ge.EID] = ge;
            ge.Spawned = true;
            foreach (Type type in ge.Listeners.Keys)
            {
                Game.World.Events.Subscribe(type, ge, ge.Listeners[type]);
            }
            return ge;
        }

        public static T Spawn<T>(Type t) where T : Entity
        {
            return (T) Spawn(t);
        }
        public static T Spawn<T>() where T : Entity, new()
        {   
            T t = new T();
            t.EID = MaxEID + 1;
            MaxEID = t.EID;
            Entities[t.EID] = t;
            t.Spawned = true;
            foreach (Type type in t.Listeners.Keys)
            {
                Game.World.Events.Subscribe(type, t, t.Listeners[type]);
            }
            return t;
        }

        public static T Spawn<T>(string s) where T : TypedEntity, new()
        {
            T t = Spawn<T>();
            t.Symbol = default(char);
            t.FG = "white";
            t.TypeName = s;
            t.Components = new Dictionary<string, EntityField<Component>>();
            if (EntityType.Types.ContainsKey(s))
            {
                EntityType et = EntityType.Types[s];
                et.Typify(t);
            }
            else
            {
                throw new InvalidOperationException(String.Format("EntityType {0} does not exist.", s));
            }
            return t;
        }

        public static T Mock<T>() where T : Entity, new()
        {
            T t = new T();
            return t;
        }

        public static T Mock<T>(string s) where T : TypedEntity, new()
        {
            T t = Mock<T>();
            t.Symbol = default(char);
            t.FG = "white";
            t.TypeName = s;
            t.Components = new Dictionary<string, EntityField<Component>>();
            if (EntityType.Types.ContainsKey(s))
            {
                EntityType et = EntityType.Types[s];
                et.MockTypify(t);
            }
            else
            {
                throw new InvalidOperationException(String.Format("EntityType {0} does not exist.", s));
            }
            return t;
        }

        public static Entity Mock(Type t)
        {
            Entity ge = (Entity)Activator.CreateInstance(t);
            return ge;
        }

        public static T Mock<T>(Type t) where T : Entity
        {
            return (T)Mock(t);
        }

        public virtual void Despawn()
		{
			Game.World.Events.Publish(new DespawnEvent {Entity = this});
			Spawned = false;
			Game.World.Events.UnsubscribeAll(this);
            Entities.Remove(EID);
            EID = -1;
        }
	}
}