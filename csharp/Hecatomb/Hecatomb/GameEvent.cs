/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/28/2018
 * Time: 9:54 AM
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of GameEvent.
	/// </summary>
	/// 
	public class GameEventHandler
	{
		public Dictionary<string, Dictionary<int, Func<GameEvent, GameEvent>>> ListenerTypes;
		
		public GameEventHandler()
		{
			ListenerTypes = new Dictionary<string, Dictionary<int, Func<GameEvent, GameEvent>>>();
			var events = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).ToList();
			foreach (var e in events)
			{
				ListenerTypes[e.Name] = new Dictionary<int, Func<GameEvent, GameEvent>>();
			}
			
		}
		public void Subscribe<T>(GameEntity g, Func<GameEvent, GameEvent> f, float priority=0)
		{
			if (!g.Spawned)
			{
				throw new InvalidOperationException(String.Format("Unspawned {0} cannot listen to events"));
			}
			var listeners = ListenerTypes[typeof(T).Name];
			if (!listeners.ContainsKey(g.EID))
			{
				Debug.WriteLine(f.GetType());
				listeners[g.EID] = f;
			}
		}
		
		public void Unsubscribe<T>(GameEntity g)
		{
			var listeners = ListenerTypes[typeof(T).Name];
			if (listeners.ContainsKey(g.EID))
			{
				listeners.Remove(g.EID);
			}
		}
		
		public void UnsubscribeAll(GameEntity g)
		{
			foreach (var listeners in ListenerTypes.Values)
			{
				if (listeners.ContainsKey(g.EID))
				{
					listeners.Remove(g.EID);
				}
			}
		}
		
		public void Publish(GameEvent g)
		{
			// !we probably have to clone this stuff in order to avoid enumeration problems.
			var listeners = ListenerTypes[g.GetType().Name];
			foreach (var listener in listeners.Values)
			{
				g = listener(g);
			}
		}
		
		public Dictionary<string, Dictionary<int, string>> StringifyListeners()
		{
			var jsonready = new Dictionary<string, Dictionary<int, string>>();
			foreach (var type in ListenerTypes.Keys)
			{
				jsonready[type] = new Dictionary<int, string>();
				foreach (var eid in ListenerTypes[type].Keys)
				{
					jsonready[type][eid] = ListenerTypes[type][eid].Method.Name;
				}
			}
			return jsonready;
		}
	}
	
	public class GameEvent{}
	
	public class PlaceEvent : GameEvent
	{
		public TypedEntity Entity;
		public int X;
		public int Y;
		public int Z;
	}
	
	public class RemoveEvent : GameEvent
	{
		public TypedEntity Entity;
		public int X;
		public int Y;
		public int Z;
	}
	
	public class SpawnEvent : GameEvent
	{
		public GameEntity Entity;
	}
	
	public class TurnBeginEvent : GameEvent
	{
		public int Turn;
	}
	
	public class TurnEndEvent : GameEvent
	{
		public int Turn;
	}
}


/// Get the Type for the class
//    Type calledType = Type.GetType(typeName);
//
//    // Invoke the method itself. The string returned by the method winds up in s
//    String s = (String)calledType.InvokeMember(
//                    methodName,
//                    BindingFlags.InvokeMethod | BindingFlags.Public | 
//                        BindingFlags.Static,
//                    null,
//                    null,
//                    null);
