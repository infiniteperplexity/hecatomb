﻿/*
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
	/// <summary>
	/// Description of GameEntity.
	/// </summary>
	public abstract class GameEntity
	{
		public int EID;
		public string ClassName;
		[JsonIgnore] public bool Spawned;
        [JsonIgnore] public Dictionary<Type, Func<GameEvent, GameEvent>> Listeners;
	
		public GameEntity()
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
	
		public virtual void Despawn()
		{
			Game.World.Events.Publish(new DespawnEvent {Entity = this});
			Spawned = false;
			Game.World.Entities.Spawned.Remove(EID);
			EID = -1;
			Game.World.Events.UnsubscribeAll(this);
		}
	}
}