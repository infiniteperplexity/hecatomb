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
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Hecatomb
{
	/// <summary>
	/// Description of Entity.
	/// </summary>
	///
	public abstract class PositionedEntity : Entity
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
        public string Highlight;
		[JsonIgnore] public bool Placed {get; private set;}

        public Dictionary<string, EntityField<Component>> Components;
        //public Dictionary<string, int> Components;
		
		public PositionedEntity() : base()
		{
            //Components = new Dictionary<string, int>();
            Components = new Dictionary<string, EntityField<Component>>();
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
				return (T) Entities[eid];
			} else {
				throw new InvalidOperationException(String.Format("{0} has no component of type {1}", this, t));
			}
		}
		
		
		public T TryComponent<T>() where T : Component
		{
			string t = typeof(T).Name;
			if (Components.ContainsKey(t)) {
				int eid = Components[t];
				return (T) Entities[eid];
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
            // I could take out one line each time I use this, if I created that IdCollection thing I was thinking about
            foreach (Component c in Components.Values)
            {
                c.AfterSelfPlace(x1, y1, z1);
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
            if (Components != null)
            {
                foreach (int c in Components.Values)
                {
                    Entities[c].Despawn();
                }
            }
            if (Placed)
            {
                Remove();
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

        public bool Solid;

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
    
	
	public class StructureEntity : PositionedEntity {

		public override void Remove()
		{
			int x0 = X;
			int y0 = Y;
			int z0 = Z;
			base.Remove();
		}
	}
}
