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
	public abstract class TileEntity : Entity
	{
		
		[JsonIgnore] public string Name;
		// might remove this...but for testing...
		[JsonIgnore] public char Symbol;
		[JsonIgnore] public string FG;
		[JsonIgnore] public string BG;
        [JsonIgnore] public bool Distinctive;
		public int X {get; private set;}
		public int Y {get; private set;}
		public int Z {get; private set;}
        protected string highlight;
        [JsonIgnore]
        public string Highlight
        {
            get
            {
                if (!Blinking)
                {
                    return highlight;
                }
                else if (DateTime.Now.Millisecond >= 500)
                {
                    return highlight;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                highlight = value;
            }
        }

        public bool Blinking; // should this be ignored?
		[JsonIgnore] public bool Placed {get; private set;}

        
        //public Dictionary<string, int> Components;
		
		public TileEntity() : base()
		{
            //Components = new Dictionary<string, int>();
            
			X = -1;
			Y = -1;
			Z = -1;
			Placed = false;
		}

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
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
            if (Game.World.Terrains[X, Y, Z].Fallable)
            {
                Fall();
            }
            // I could take out one line each time I use this, if I created that IdCollection thing I was thinking about 
        }

        public Coord FindPlace(int x, int y, int z = -1, int max = 5, int min = 0, bool groundLevel = true, Func<int, int, int, bool> valid = null)
        {
            return new Coord(1, 1, 1);
        }

        public static Dictionary<TileEntity,Coord> FindPlaces(List<TileEntity> entities, int x, int y, int z = -1, int max = 5, int min = 0, bool groundLevel = true, Func<int, int, int, bool> valid = null)
        {
            return new Dictionary<TileEntity, Coord>();
        }
        public virtual void Fall()
        {

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
            if (Placed)
            {
                Remove();
            }
            base.Despawn();
        }
		
        public virtual string GetDisplayName()
        {
            return Name;
        }

		public virtual string Describe(
			bool article=true,
			bool definite=false,
			bool capitalized=false
		)
		{
			string name = GetDisplayName();
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
}
