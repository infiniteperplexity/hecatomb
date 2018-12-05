﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:00 PM
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of EntitySpawner.
	/// </summary>
	public class EntityHandler
	{
		public int MaxEID;
		public Dictionary<int, GameEntity> Spawned;
		// may do a pool at some point
		
		public EntityHandler()
		{
			Spawned = new Dictionary<int, GameEntity>();
			MaxEID = -1;
		}
		
		public GameEntity Spawn(Type t)
		{
			GameEntity ge = (GameEntity) Activator.CreateInstance(t);
			ge.EID = MaxEID + 1;
			MaxEID = ge.EID;
			Spawned[ge.EID] = ge;
			ge.Spawned = true;
            foreach (Type type in ge.Listeners.Keys)
            {
                Game.World.Events.Subscribe(type, ge, ge.Listeners[type]);
            }
            return ge;
		}
		public T Spawn<T>() where T : GameEntity, new()
		{
			T t = new T();
			t.EID = MaxEID + 1;
			MaxEID = t.EID;
			Spawned[t.EID] = t;
			t.Spawned = true;
            foreach (Type type in t.Listeners.Keys)
            {
                Game.World.Events.Subscribe(type, t, t.Listeners[type]);
            }
			return t;
		}
			
		public T Spawn<T>(string s) where T: PositionedEntity, new()
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
			} else {
				throw new InvalidOperationException(String.Format("EntityType {0} does not exist.",s));
			}
			return t;
		}
			
		public T Mock<T>() where T : GameEntity, new()
		{
			T t = new T();
			return t;
		}

        public T Mock<T>(string s) where T : PositionedEntity, new()
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

        public GameEntity Mock(Type t)
		{
			GameEntity ge = (GameEntity) Activator.CreateInstance(t);
			return ge;
		}
	}
}
