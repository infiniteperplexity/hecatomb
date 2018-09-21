/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 11:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
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
		public string[] Required;
		
		public Component()
		{
			Required = new string[0];
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
			Required = new string[] {"Position"};
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
	
	public class Movement : Component
	{
		public bool Walks;
		public bool Climbs;
		public bool Flies;
		public bool Swims;
		
		public Movement()
		{
			Required = new string[] {"Position"};
			Walks = true;
			Climbs = true;
			Flies = false;
			Swims = true;
		}
		
		public bool CanMove(int x1, int y1, int z1)
		{
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH) {
				return false;
			}
			Terrain tile = Game.World.tiles[x1, y1, z1];
			
			if (tile.Solid) {
				return false;
			}
			if (tile.Fallable && Flies==false) {
				return false;
			}
			return true;
		}
		
		public void StepTo(int x1, int y1, int z1)
		{
			Entity.x = x1;
			Entity.y = y1;
			Entity.z = z1;
		}
	}
}
