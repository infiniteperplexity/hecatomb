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

namespace Hecatomb
{
	class NoSaveAttribute : Attribute
	{
		
	}
	/// <summary>
	/// Description of Component.
	/// </summary>
	/// 
	
	public abstract class Component : GameEntity
	{
		private int EntityEID;
		[JsonIgnore] public TypedEntity Entity
		{
			get
			{
				return (TypedEntity) Game.World.Entities.Spawned[EntityEID];
			}
			set
			{
				EntityEID = value.EID;
			}
		}
		[JsonIgnore] public string[] Required;
		
		public Component() : base()
		{
			EntityEID = -1;
			Required = new string[0];
		}
		
		public void AddToEntity(TypedEntity e)
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
	}
}
