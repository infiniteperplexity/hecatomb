/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 11:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using ShadowCaster;

namespace Hecatomb
{
	/// <summary>
	/// Description of Component.
	/// </summary>
	/// 
	
	public class Component
	{
		public TypedEntity Entity;
		protected Type[] required;
		
		public Component()
		{
			required = new Type[0];
		}
		
		public void AddToEntity(TypedEntity e)
		{
			e.Components[this.GetType()] = this;
			Entity = e;
			this.OnAddToEntity();
		}
		public void OnAddToEntity()
		{
		}
		public void RemoveFromEntity()
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
	
	public class Senses : Component
	{
		public int Range;
		public HashSet<Tuple<int,int,int>> Visible;
		private int storedZ;
		
		public Senses()
		{
			required = new Type[] {typeof(Position)};
			Range = 10;
		}
		
		public HashSet<Tuple<int,int,int>> GetFOV()
		{
			resetVisible();
			int x = Entity.x;
			int y = Entity.y;
			storedZ = Entity.z;
			ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, addToVisible);
			return Visible;
		}
		
		public bool CanSeeThrough(int x, int y) {
			return !cannotSeeThrough(x, y);
		}
		
		private bool cannotSeeThrough(int x, int y) {
			return Game.World.tiles[x, y, storedZ].Opaque;
		}
		private void resetVisible()
		{
			Visible = new HashSet<Tuple<int, int, int>>();
		}
		
		private void addToVisible(int x, int y)
		{
			Visible.Add(new Tuple<int, int, int>(x, y, storedZ));
		}
	}
}
