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
		public Dictionary<Type, Dictionary<GameEntity, Func<GameEvent, GameEvent>>> ListenerTypes;
		public GameEventHandler()
		{
			ListenerTypes = new Dictionary<Type, Dictionary<GameEntity, Func<GameEvent, GameEvent>>>();
			var events = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).ToList();
			foreach (var e in events)
			{
				ListenerTypes[e] = new Dictionary<GameEntity, Func<GameEvent, GameEvent>>();
			}
		}
		public void Subscribe<T>(GameEntity g, Func<GameEvent, GameEvent> f, float priority=0)
		{
			var listeners = ListenerTypes[typeof(T)];
			if (!listeners.ContainsKey(g))
			{
				listeners[g] = f;
			}
		}
		
		public void Unsubscribe<T>(GameEntity g)
		{
			var listeners = ListenerTypes[typeof(T)];
			if (listeners.ContainsKey(g))
			{
				listeners.Remove(g);
			}
		}
		
		public void UnsubscribeAll(GameEntity g)
		{
			foreach (var listeners in ListenerTypes.Values)
			{
				if (listeners.ContainsKey(g))
				{
					listeners.Remove(g);
				}
			}
		}
		
		public void Publish(GameEvent g)
		{
			// !we probably have to clone this stuff in order to avoid enumeration problems.
			var listeners = ListenerTypes[g.GetType()];
			foreach (var listener in listeners.Values)
			{
				g = listener(g);
			}
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
}
