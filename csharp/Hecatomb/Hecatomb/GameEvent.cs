/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/26/2018
 * Time: 10:58 AM
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

//namespace Hecatomb
//{
//	/// <summary>
//	/// Description of GameEvent.
//	/// </summary>
//	public abstract class GameEvent
//	{
//		public TypedEntity Entity;
//	}
//	public class PlaceEvent : GameEvent
//	{
//		public int x;
//		public int y;
//		public int z;
//	}
//	
//	public class GameEventHandler
//	{
//		public Dictionary<Type,List<GameEntity>> Listeners;
//		public GameEventHandler()
//		{
//			Listeners = new Dictionary<Type,List<GameEntity>>();
//			var etypes = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).ToList();
//			foreach (var etype in etypes)
//			{
//				Listeners[etype] = new List<GameEntity>();
//			}
//		}
//		public GameEvent Publish(GameEvent ge)
//		{
//			var gt = (ge.GetType()) ge;
//			TypedEntity t = ge.Entity;
//			if (t!=null)
//			{
//				t.OnSelfEvent(ge);
//				foreach (Component c in t.Components.Values)
//				{
//					c.OnEntityEvent(ge);
//				}
//			}
//			var listeners = Listeners[ge.GetType()];
//			foreach (GameEntity listener in listeners)
//			{
//				Debug.WriteLine("are we at least getting this far?");
//				ge = listener.OnGameEvent(ge);
//			}
//			return ge;
//		}
//		public void Subscribe<T>(GameEntity e)
//		{
//			var listeners = Listeners[typeof(T)];
//			if (!listeners.Contains(e))
//			{
//				listeners.Add(e);
//			}
//			
//		}
//		public void Unsubscribe<T>(GameEntity e)
//		{
//			var listeners = Listeners[typeof(T)];
//			if (listeners.Contains(e))
//			{
//				listeners.Remove(e);
//			}
//		}
//		public void UnsubscribeAll(GameEntity e)
//		{
//			foreach (var kv in Listeners)
//			{
//				var listeners = kv.Value;
//				if (listeners.Contains(e))
//				{
//					listeners.Remove(e);
//				}
//			}
//		}
//		
//	}
//}
