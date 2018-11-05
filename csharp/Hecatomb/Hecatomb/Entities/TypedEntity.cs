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
	public abstract partial class PositionedEntity : GameEntity
	{
		public string TypeName;
		[JsonIgnore] public string Name;
		// might remove this...but for testing...
		[JsonIgnore] public char Symbol;
		[JsonIgnore] public string FG;
		[JsonIgnore] public string BG;
		public int X {get; private set;}
		public int Y {get; private set;}
		public int Z {get; private set;}
		[JsonIgnore] public bool Placed {get; private set;}

		public Dictionary<string, int> Components;
		
		public PositionedEntity() : base()
		{
			X = -1;
			Y = -1;
			Z = -1;
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
		
		public virtual void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
			if (!Spawned)
			{
				throw new InvalidOperationException(String.Format("Cannot place {0} that hasn't been spawned yet.",this));
			}
			if (Placed)
			{
				this.Remove();
			}
			X = x1;
			Y = y1;
			Z = z1;
			Placed = true;
			if (fireEvent)
			{
				Game.World.Events.Publish(new PlaceEvent() {Entity = this, X = x1, Y = y1, Z = z1});
			}
		}
		
		public virtual void Remove()
		{
			Game.World.Events.Publish(new RemoveEvent() {Entity = this, X = X, Y = Y, Z = Z});
			X = -1;
			Y = -1;
			Z = -1;
			Placed = false;
		}
		
		public virtual void Destroy()
		{
			Remove();
			Despawn();
		}

        public override void Despawn()
        {
            foreach (int c in Components.Values)
            {
                Game.World.Entities.Spawned[c].Despawn();
            }
            base.Despawn();
        }
		
		public virtual string Describe(
			bool article=true,
			bool definite=false,
			bool capitalized=false
		)
		{
			string name = Name;
			bool vowel = false;
			if (name==null)
			{
				return "";
			}
			if ("aeiou".Contains(char.ToLower(name[0]).ToString()))
			{
				vowel = true;
			}
			if (article || definite)
			{
				if (definite)
				{
					name = "the " + name;
				}
				else
				{
					if (vowel)
					{
						name = "an " + name;
					}
					else
					{
						name = "a " + name;
					}
				}
			}
			if (capitalized)
			{
				name = char.ToUpper(name[0]) + name.Substring(1);
			}
			return name;
		}
	}
	
	public class Creature : PositionedEntity {
		
		public override void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
			Creature e = Game.World.Creatures[x1,y1,z1];
			
			if (e==null)
			{
				if (!fireEvent)
				{
					Debug.Print("about to try placing {0} at {1} {2} {3} without firing an event", this, x1, y1, z1);
				}
				Game.World.Creatures[x1,y1,z1] = this;
				base.Place(x1, y1, z1, fireEvent);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
				));
			}
		}
		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
			Game.World.Creatures[x0,y0,z0] = null;
		}
	}
	public class Feature : PositionedEntity {

		public override void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
			Feature e = Game.World.Features[x1,y1,z1];
			if (e==null)
			{
				Game.World.Features[x1,y1,z1] = this;
				base.Place(x1, y1, z1, fireEvent);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
				));
			}
		}
		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
			Game.World.Features[x0,y0,z0] = null;
		}
	}
    public class Item : PositionedEntity {

        public bool Owned;
        public Dictionary<string, int> Resources;
        public Dictionary<string, int> Claims;

        public Item() : base()
        {
            Resources = new Dictionary<string, int>();
        }
		
		public override void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
			Item e = Game.World.Items[x1,y1,z1];
			if (e==null)
			{
				Game.World.Items[x1,y1,z1] = this;
				base.Place(x1, y1, z1, fireEvent);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
				));
			}
		}
		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
			Game.World.Items[x0,y0,z0] = null;
		}
		
		public override void Despawn()
		{
			if (Placed)
			{
				Remove();
			}
			base.Despawn();
		}

        public void AddResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                AddResource((resource, resources[resource]));
            }
        }
        public void AddResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                AddResource(tuple);
            }
        }
        public void AddResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                AddResource(tuple);
            }
        }
        public void AddResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                Resources[resource.Item1] = 0;
            }
            Resources[resource.Item1] += resource.Item2;
        }

        public void RemoveResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                throw new InvalidOperationException();
            }
            else if (Resources[resource.Item1]<resource.Item2)
            {
                throw new InvalidOperationException();
            }
            else
            {
                Resources[resource.Item1] -= resource.Item2;
                if (Resources[resource.Item1]==0)
                {
                    Resources.Remove(resource.Item1);
                }
                if (Resources.Keys.Count==0)
                {
                    Despawn();
                }
            }
        }
        public void RemoveResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                RemoveResource(tuple);
            }
        }
        public void RemoveResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                RemoveResource(tuple);
            }
        }
        public void RemoveResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                RemoveResource((resource, resources[resource]));
            }
        }

        public void UnclaimResource(ValueTuple<string, int> resource)
        {
            if (!Claims.ContainsKey(resource.Item1))
            {
                throw new InvalidOperationException();
            }
            else if (Claims[resource.Item1] < resource.Item2)
            {
                throw new InvalidOperationException();
            }
            else
            {
                Claims[resource.Item1] -= resource.Item2;
                if (Claims[resource.Item1] == 0)
                {
                    Claims.Remove(resource.Item1);
                }
            }
        }
        public void UnclaimResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                UnclaimResource(tuple);
            }
        }
        public void UnclaimResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                UnclaimResource(tuple);
            }
        }
        public void UnclaimResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                UnclaimResource((resource, resources[resource]));
            }
        }


        public bool HasResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                return false;
            }
            else
            {
                return (Resources[resource.Item1] >= resource.Item2);
            }
        }
        public bool HasResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                if (!HasResource(tuple))
                {
                    return false;
                }
            }
            return true;
        }
        public bool HasResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                if (!HasResource(tuple))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                if (!HasResource((resource, resources[resource])))
                {
                    return false;
                }
            }
            return true;
        }

        public static void PlaceResource(ValueTuple<string, int> resource, int x, int y, int z)
        {
            Item it = Game.World.Items[x, y, z];
            if (it == null)
            {
                it = Game.World.Entities.Spawn<Item>();
                it.Place(x, y, z);
            }
            it.AddResource(resource);
        }
        public static void PlaceResources(ValueTuple<string, int>[] resources, int x, int y, int z)
        {
            foreach (var tuple in resources)
            {
                PlaceResource(tuple, x, y, z);
            }
        }
        public static void PlaceResources(List<ValueTuple<string, int>> resources, int x, int y, int z)
        {
            foreach (var tuple in resources)
            {
                PlaceResource(tuple, x, y, z);
            }
        }
        public static void PlaceResources(Dictionary<string, int> resources, int x, int y, int z)
        {
            foreach (string resource in resources.Keys)
            {
                PlaceResource((resource, resources[resource]), x, y, z);
            }
        }
    }
	public class TaskEntity : PositionedEntity {

		public override void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
			TaskEntity e = Game.World.Tasks[x1,y1,z1];
			if (e==null)
			{
				Game.World.Tasks[x1,y1,z1] = this;
				base.Place(x1, y1, z1, fireEvent);
			}
			else 
			{
				throw new InvalidOperationException(String.Format(
					"Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
				));
			}
		}
		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
			Game.World.Tasks[x0,y0,z0] = null;
		}
	}
	
	public class StructureEntity : PositionedEntity {

		public override void Place(int x1, int y1, int z1, bool fireEvent=true)
		{
//			if (e==null)
//			{
//				base.Place(x1, y1, z1, fireEvent);
//			}
//			else 
//			{
//				throw new InvalidOperationException(String.Format(
//					"Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
//				));
//			}
		}
		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
		}
	}
}
