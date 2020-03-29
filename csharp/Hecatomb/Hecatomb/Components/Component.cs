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
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	public abstract class Component : Entity
	{
        public TypedEntityField<TypedEntity> Entity = new TypedEntityField<TypedEntity>();
        [JsonIgnore] public string[] Required = new string[0];
		
		public void AddToEntity(TypedEntity e)
		{
			if (!Spawned)
			{
				throw new InvalidOperationException(String.Format("Cannot add {0} that has not been spawned.",this));
			}
            if (EID==-1)
            {
                Debug.WriteLine("This should not be happening!");
            }
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				e.Components[this.GetType().Name] = this.EID;
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				e.Components[this.GetType().BaseType.Name] = this.EID;
			}
            Entity = e;
        }

        // this method is kind of a disaster and doesn't work at all...what to do???
        public void AddToMockEntity(TypedEntity e)
        {
            // if it's a plain old Component subclass, use its own type as the key
            if (this.GetType().BaseType == typeof(Component))
            {
                //e.Components[this.GetType().Name] = this.EID;
            }
            else
            {
                // if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
                //e.Components[this.GetType().BaseType.Name] = this.EID;
            }
            //Entity = e;
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
			Entity.EID = -1;
		}

        public virtual void OnAddToEntity() {}
        public virtual void InterpretJSON(string json) {}
        public virtual void AfterSelfPlace(int x, int y, int z) {}
	}
}
