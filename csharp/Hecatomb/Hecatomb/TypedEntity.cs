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

namespace Hecatomb
{
	/// <summary>
	/// Description of Entity.
	/// </summary>
	/// 
	
	public abstract class GameEntity : ISaveable
	{
		public int EID;

		public GameEntity()
		{

		}
		
		public void Publish(GameEvent g)
		{
			
		}
		
		public virtual string Stringify()
		{
			return "huh?";
		}
		
		public string StringifyReference()
		{
			return "{\"EID\":" + EID +"}";
		}
		
		public void Despawn()
		{
			Game.World.Entities.Spawned.Remove(this);
		}
	}
	public abstract class TypedEntity : GameEntity
	{
		public string EType;
		public string Name;
		// might remove this...but for testing...
		public char Symbol;
		public string FG;
		public string BG;
		public int x {get; private set;}
		public int y {get; private set;}
		public int z {get; private set;}
		public bool Placed {get; private set;}

		public Dictionary<Type, Component> Components;
		
		public TypedEntity() : base()
		{
			x = -1;
			y = -1;
			z = -1;
			Placed = false;
		}
		public T GetComponent<T>() where T : Component
		{
			Type t = typeof(T);
			if (Components.ContainsKey(t)) {
				return (T) Components[t];
			} else {
				throw new InvalidOperationException();
			}
		}
		
		
		public T TryComponent<T>() where T : Component
		{
			Type t = typeof(T);
			if (Components.ContainsKey(t)) {
				return (T) Components[t];
			} else {
				return default(T);
			}
		}
		
		public virtual void Place(int x1, int y1, int z1)
		{
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
		
		public override string Stringify()
		{
			return
				"{"
					+ "\"Type\": " + EType + ", "
					+ "\"x\": " + x + ", "
					+ "\"y\": " + y + ", "
					+ "\"z\": " + z + ", "
					+ "\"Components\": " + "[]" +
				"}";
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
