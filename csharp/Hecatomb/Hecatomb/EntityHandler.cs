/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:00 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of EntitySpawner.
	/// </summary>
	public class EntityHandler
	{
		public int MaxEID;
		public HashSet<GameEntity> Spawned;
		
		public EntityHandler()
		{
			Spawned = new HashSet<GameEntity>();
			MaxEID = -1;
		}
		public T Spawn<T>() where T : GameEntity, new()
		{
			T t = new T();
			t.EID = MaxEID + 1;
			MaxEID = t.EID;
			Spawned.Add(t);
			return t;
		}
			
		public T Spawn<T>(string s) where T: TypedEntity, new()
		{
			T t = Spawn<T>();
			t.Symbol = default(char);
			t.FG = "white";
			t.EType = s;
			t.Components = new Dictionary<Type, Component>();
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
	}
}
