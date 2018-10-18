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
	/// <summary>
	/// Description of GameEntity.
	/// </summary>
	public abstract class GameEntity
	{
		public int EID;
		public string ClassName;
		[JsonIgnore] public bool Spawned;
	
		public GameEntity()
		{
			ClassName = this.GetType().Name;
			EID = -1;
			Spawned = false;
		}
			
		public virtual GameEvent OnSelfSpawn(GameEvent g)
		{
			return g;
		}
			
		public void Publish(GameEvent g)
		{
				
		}
	
		public virtual void Despawn()
		{
			Spawned = false;
			Game.World.Entities.Spawned.Remove(EID);
			EID = -1;
		}
	}
}