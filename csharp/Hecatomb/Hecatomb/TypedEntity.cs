/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Entity.
	/// </summary>
	/// 
	
	public abstract class GameEntity
	{
		public int EID;
		[JsonIgnore] public bool Spawned;

		public GameEntity()
		{
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

		public void Despawn()
		{
			//Game.World.Entities.Spawned.Remove(this);
		}
	}
	public abstract class TypedEntity : GameEntity
	{
		public string EType;
		[JsonIgnore] public string Name;
		// might remove this...but for testing...
		[JsonIgnore] public char Symbol;
		[JsonIgnore] public string FG;
		[JsonIgnore] public string BG;
		public int x {get; private set;}
		public int y {get; private set;}
		public int z {get; private set;}
		[JsonIgnore] public bool Placed {get; private set;}

		public Dictionary<string, int> Components;
		
		public TypedEntity() : base()
		{
			x = -1;
			y = -1;
			z = -1;
			Placed = false;
		}
		public T GetComponent<T>() where T : Component
		{
			string t = typeof(T).Name;
			if (Components.ContainsKey(t)) {
				int eid = Components[t];
				return (T) Game.World.Entities.Spawned[eid];
			} else {
				throw new InvalidOperationException(String.Format("{0} has no component of type {1}", this, t));
			}
		}
		
		
		public T TryComponent<T>() where T : Component
		{
			string t = typeof(T).Name;
			if (Components.ContainsKey(t)) {
				int eid = Components[t];
				return (T) Game.World.Entities.Spawned[eid];
			} else {
				return default(T);
			}
		}
		
		public virtual void Place(int x1, int y1, int z1)
		{
			if (!Spawned)
			{
				throw new InvalidOperationException(String.Format("Cannot place {0} that hasn't been spawned yet.",this));
			}
			if (Placed)
			{
				this.Remove();
			}
			x = x1;
			y = y1;
			z = z1;
			Placed = true;
			Game.World.Events.Publish(new PlaceEvent() {Entity = this, X = x1, Y = y1, Z = z1});
		}
		public virtual void Remove()
		{
			Game.World.Events.Publish(new RemoveEvent() {Entity = this, X = x, Y = y, Z = z});
			x = -1;
			y = -1;
			z = -1;
			Placed = false;
		}
	}
	
	public class Creature : TypedEntity {
		
		public override void Place(int x1, int y1, int z1)
		{
			Creature e = Game.World.Creatures[x1,y1,z1];
			if (e==null)
			{
				Game.World.Creatures[x1,y1,z1] = this;
				base.Place(x1, y1, z1);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", EType, x1, y1, z1, e.EType
				));
			}
		}
		public override void Remove()
		{
			int x0 = x;
			int y0 = y;
			int z0 = z;
			base.Remove();
			Game.World.Creatures[x0,y0,z0] = null;
		}
	}
	public class Feature : TypedEntity {

		public override void Place(int x1, int y1, int z1)
		{
			Feature e = Game.World.Features[x1,y1,z1];
			if (e==null)
			{
				Game.World.Features[x1,y1,z1] = this;
				base.Place(x1, y1, z1);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", EType, x1, y1, z1, e.EType
				));
			}
		}
		public override void Remove()
		{
			int x0 = x;
			int y0 = y;
			int z0 = z;
			base.Remove();
			Game.World.Features[x0,y0,z0] = null;
		}
	}
	public class Item : TypedEntity {

		public override void Place(int x1, int y1, int z1)
		{
			Item e = Game.World.Items[x1,y1,z1];
			if (e==null)
			{
				Game.World.Items[x1,y1,z1] = this;
				base.Place(x1, y1, z1);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", EType, x1, y1, z1, e.EType
				));
			}
		}
		public override void Remove()
		{
			int x0 = x;
			int y0 = y;
			int z0 = z;
			base.Remove();
			Game.World.Items[x0,y0,z0] = null;
		}
	}
	public class TaskEntity : TypedEntity {

		public override void Place(int x1, int y1, int z1)
		{
			TaskEntity e = Game.World.Tasks[x1,y1,z1];
			if (e==null)
			{
				Game.World.Tasks[x1,y1,z1] = this;
				base.Place(x1, y1, z1);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", EType, x1, y1, z1, e.EType
				));
			}
		}
		public override void Remove()
		{
			int x0 = x;
			int y0 = y;
			int z0 = z;
			base.Remove();
			Game.World.Tasks[x0,y0,z0] = null;
		}
	}
}
