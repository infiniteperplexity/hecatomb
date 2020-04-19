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
        Dictionary<string, EntityField<Component>>? _components;
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
            return (T)_components![t].UnboxBriefly()!;
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
            if (_components is null)
            {
                _components = new Dictionary<string, EntityField<Component>>();
            }
            _components[c.GetType().Name] = c;
            c.AddToEntity(this);
        }
    }

}
