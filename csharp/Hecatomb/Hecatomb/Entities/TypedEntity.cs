using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class TypedEntity : TileEntity
    {
        public string TypeName;
        public Dictionary<string, EntityField<Component>> Components;


        public TypedEntity() : base()
        {
            Components = new Dictionary<string, EntityField<Component>>();
        }

        public T GetComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components.ContainsKey(t))
            {
                int eid = Components[t];
                return (T)Entities[eid];
            }
            else
            {
                throw new InvalidOperationException(String.Format("{0} has no component of type {1}", this, t));
            }
        }


        public T TryComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components.ContainsKey(t))
            {
                int eid = Components[t];
                return (T)Entities[eid];
            }
            else
            {
                return default(T);
            }
        }

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            base.Place(x1, y1, z1, fireEvent);
            foreach (Component c in Components.Values)
            {
                c.AfterSelfPlace(x1, y1, z1);
            }
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
            base.Despawn();
        }    
    }
}
