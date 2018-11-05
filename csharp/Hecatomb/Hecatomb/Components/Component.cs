/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 11:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of Component.
	/// </summary>
	/// 
	
	public abstract class Component : GameEntity
	{
		[JsonProperty] private int EntityEID;
		[JsonIgnore] public PositionedEntity Entity
		{
			get
			{
				if (EntityEID==-1)
				{
					return null;
				}
				else
				{
					return (PositionedEntity) Game.World.Entities.Spawned[EntityEID];
				}
			}
			set
			{
				if (value==null)
				{
					EntityEID = -1;
				}
				else
				{
					EntityEID = value.EID;
				}
			}
		}
		[JsonIgnore] public string[] Required;
		
		public Component() : base()
		{
			EntityEID = -1;
			Required = new string[0];
		}
		
		public void AddToEntity(PositionedEntity e)
		{
			if (!Spawned)
			{
				throw new InvalidOperationException(String.Format("Cannot add {0} that has not been spawned.",this));
			}
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				e.Components[this.GetType().Name] = this.EID;
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				e.Components[this.GetType().BaseType.Name] = this.EID;
			}
			EntityEID = e.EID;
		}
		
//		public virtual GameEvent OnEntityEvent(GameEvent ge)
//		{
//			return ge;
//		}
				
		public void OnAddToEntity()
		{
		}
		public void RemoveFromEntity()
		{
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				Entity.Components.Remove(this.GetType().Name);
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				Entity.Components.Remove(this.GetType().BaseType.Name);
			}
			EntityEID = -1;
		}
		
		public virtual void ApplyParameters(string json)
		{
		}
	}
}
