using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    // ComposedEntity are entity, such as creature or features, that can have Components attached to add additional behaviors
    public abstract class ComposedEntity : TileEntity
    {
        public Dictionary<string, ListenerHandledEntityPointer<Component>>? _components;
        [JsonIgnore] protected List<Component> Components;

        protected ComposedEntity() : base()
        {
            Components = new List<Component>();
            //Components.Add(new Movement());
        }

        public void SpawnComponents()
        {
            foreach (var c in Components)
            {
                AddComponent(Entity.Spawn<Component>(c));
            }
        }
        public T GetComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (_components is null || !_components.ContainsKey(t))
            {
                throw new InvalidOperationException($"{this} has no component of type {t}");
            }
            // let's not update nullity here; this could easily get called by the interface
            return (T)_components![t].UnboxIfNotNull()!;
        }

        public bool HasComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (_components is null || !_components.ContainsKey(t))
            {
                return false;
            }
            return false;
        }

        public void AddComponent(Component c)
        {
            if (!Spawned)
            {
                throw new InvalidOperationException("Using Component.Add in the constructor, not AddComponent.");
            }
            if (_components is null)
            {
                _components = new Dictionary<string, ListenerHandledEntityPointer<Component>>();
            }
            
            _components[c.GetType().Name] = c;
            c.AddToEntity(this);
        }
    }

}
