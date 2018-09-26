/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/26/2018
 * Time: 1:46 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of AltEvents.
	/// </summary>
	public class GameEvent
	{	
		public TypedEntity Entity;
		static List<GameEntity> Listeners;
		
		static GameEvent()
		{
			Listeners = new List<GameEntity>();
		}
		public GameEvent()
		{
		}
		
		static void Subscribe(GameEntity ge)
		{
			if (!Listeners.Contains(ge))
			{
				Listeners.Add(ge);
			}
		}
		static void Unsubscribe(GameEntity ge)
		{
			if (Listeners.Contains(ge))
			{
				Listeners.Remove(ge);
			}
		}
		public static void Publish(GameEvent ge)
		{
			TypedEntity te = ge.Entity;
			ge.SelfTrigger(te);
			foreach (GameEntity listener in Listeners)
			{
				ge.Trigger(listener);
			}
		}
		void Trigger(GameEntity g)
		{
			g.OnEvent(this);
		}
		public virtual void SelfTrigger(TypedEntity t)
		{
			t.OnSelfEvent(this);
		}
		void EntityTrigger(Component c)
		{
			c.OnEntityEvent(this);
		}		
	}
	
	public class PlaceEvent : GameEvent
	{
		public int x, y, z;
		public override void SelfTrigger(TypedEntity t){t.OnSelfEvent(this);}
	}
	
	public abstract partial class GameEntity
	{
		public virtual void OnEvent(GameEvent ge) {}
		public virtual void OnEvent(PlaceEvent ge) {}
	}
	public abstract partial class TypedEntity
	{
		public virtual void OnSelfEvent(GameEvent ge) {}
		public virtual void OnSelfEvent(PlaceEvent ge) {}
	}
	public abstract partial class Component
	{
		public virtual void OnEntityEvent(GameEvent ge) {}
		public virtual void OnEntityEvent(PlaceEvent ge) {}
	}
}
