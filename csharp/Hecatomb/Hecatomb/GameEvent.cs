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
		public Dictionary<int, Func<GameEvent, GameEvent>> GlobalListeners;
		
		public GameEventHandler()
		{
			GlobalListeners = new Dictionary<int, Func<GameEvent, GameEvent>>();
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
				throw new InvalidOperationException(String.Format("Unspawned {0} cannot listen to events", g));
			}
			if (typeof(T)==typeof(GameEvent))
			{
				if (!GlobalListeners.ContainsKey(g.EID))
				{
					GlobalListeners[g.EID] = f;
				}
			}
			else
			{
				var listeners = ListenerTypes[typeof(T).Name];
				if (!listeners.ContainsKey(g.EID))
				{
					listeners[g.EID] = f;
				}
			}
		}
		
		
		public void Unsubscribe<T>(GameEntity g)
		{
			if (typeof(T)==typeof(GameEvent))
			{
				if (GlobalListeners.ContainsKey(g.EID))
				{
					GlobalListeners.Remove(g.EID);
				}
			}
			else
			{
				var listeners = ListenerTypes[typeof(T).Name];
				if (listeners.ContainsKey(g.EID))
				{
					listeners.Remove(g.EID);
				}
			}
		}
		
		public void UnsubscribeAll(GameEntity g)
		{
			if (GlobalListeners.ContainsKey(g.EID))
			{
				GlobalListeners.Remove(g.EID);
			}
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
			foreach (var listener in GlobalListeners.Values)
			{
				g = listener(g);
			}
			var listeners = ListenerTypes[g.GetType().Name];
			foreach (var listener in listeners.Values)
			{
				g = listener(g);
			}
            g.Fire();
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
			jsonready["GameEvent"] = new Dictionary<int, string>();
			foreach (var eid in GlobalListeners.Keys)
			{
					jsonready["GameEvent"][eid] = GlobalListeners[eid].Method.Name;
			}
			return jsonready;
		}
	}
	
	public class GameEvent{

        public virtual void Fire()
        {

        }
    }
	
	public class PlaceEvent : GameEvent
	{
		public PositionedEntity Entity;
		public int X;
		public int Y;
		public int Z;
	}
	
	public class RemoveEvent : GameEvent
	{
		public PositionedEntity Entity;
		public int X;
		public int Y;
		public int Z;
	}
	
	public class SpawnEvent : GameEvent
	{
		public GameEntity Entity;
	}
	
	public class DespawnEvent : GameEvent
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
	
	public class PlayerActionEvent : GameEvent
	{
		public string ActionType;
		public Dictionary<string, object> Details;
	}
	
	public class ContextChangeEvent : GameEvent
	{
        public string Note;
        public ControlContext OldContext;
		public ControlContext NewContext;
	}

    public class TutorialEvent : GameEvent
    {
        public string Action;
    }
	
	public class AttackEvent : GameEvent
	{
        public Attacker Attacker;
        public Defender Defender;
        public int Roll;
        public Dictionary<string, int> Modifiers;
	}

    public class AchievementEvent : GameEvent
    {
        public string Action;
    }

    public class SensoryEvent: GameEvent
    {
        public int X;
        public int Y;
        public int Z;
        public string Sight;
        public string Sound;

        public override void Fire()
        {
            // a lot more conditionals than this...
            Game.StatusPanel.PushMessage(Sight);
        }
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
