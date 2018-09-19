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
		public TypedEntity Entity;
		
		public void addToEntity(TypedEntity e)
		{
			e.Components[this.GetType()] = this;
			Entity = e;
			this.OnAddToEntity();
		}
		public void OnAddToEntity()
		{
		}
		public void removeFromEntity()
		{
			this.OnRemoveFromEntity();
			Entity.Components.Remove(this.GetType());
			Entity = null;
		}
		public void OnRemoveFromEntity()
		{
			
		}
		
		protected string GetThisClassName() { return this.GetType().Name; }
	}
	
	public class Position : Component
	{
		public int x;
		public int y;
		public int z;
	}
}
