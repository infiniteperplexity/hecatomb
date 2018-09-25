/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 11:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Component.
	/// </summary>
	/// 
	
	public class Component
	{
		public Entity Entity;
		public string[] Required;
		
		public Component()
		{
			Required = new string[0];
		}
		
		public void AddToEntity(Entity e)
		{
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				e.Components[this.GetType()] = this;
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				e.Components[this.GetType().BaseType] = this;
			}
			
			Entity = e;
			this.OnAddToEntity();
		}
		public void OnAddToEntity()
		{
		}
		public void RemoveFromEntity()
		{
			this.OnRemoveFromEntity();
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				Entity.Components.Remove(this.GetType());
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				Entity.Components.Remove(this.GetType().BaseType);
			}
			Entity = null;
		}
		public void OnRemoveFromEntity()
		{
			
		}
	}
}
