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
        public Dictionary<string, int>? _components;
        [JsonIgnore] protected List<Component> Components;

        protected ComposedEntity() : base()
        {
            Components = new List<Component>();
        }

        public virtual GameEvent OnDespawn(GameEvent ge)
        {
            var de = (DespawnEvent)ge;
            if (de.Entity!.EID is null)
            {
                return ge;
            }
            if (_components != null && _components.ContainsValue((int)de.Entity!.EID!))
            {
                string t = de.Entity!.GetType().Name;
                if (_components.ContainsKey(t))
                {
                    _components.Remove(t);
                }
            }
            return ge;
        }

        public T GetPrespawnComponent<T>() where T: Component
        {
            foreach (var c in Components)
            {
                if (c is T)
                {
                    return (T)c;
                }
            }
            // definitely risking a crash here
            throw new InvalidOperationException($"{this} has no component of type {typeof(T).Name}");
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
            // is this even a good idea?  it seems like it could cause problems down the line
            if (!Spawned)
            {
                return GetPrespawnComponent<T>();
            }
            string t = typeof(T).Name;
            if (_components is null || !_components.ContainsKey(t))
            {
                throw new InvalidOperationException($"{this} has no component of type {t}");
            }
            int eid = _components[t];
            if (!GameState.World!.Entities.ContainsKey(eid))
            {
                throw new InvalidOperationException($"{this} has no component of type {t}");
            }
            // let's not update nullity here; this could easily get called by the interface
            return (T)GameState.World!.Entities[eid];
        }

        public T? TryComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (_components is null || !_components.ContainsKey(t))
            {
                return null;
            }
            int eid = _components[t];
            if (!GameState.World!.Entities.ContainsKey(eid))
            {
                return null;
            }
            // let's not update nullity here; this could easily get called by the interface
            return (T)GameState.World!.Entities[eid];
        }

        public bool HasComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (_components is null || !_components.ContainsKey(t))
            {
                return false;
            }
            return true;
        }

        public void AddComponent(Component c)
        {
            if (!Spawned)
            {
                throw new InvalidOperationException("Using Component.Add in the constructor, not AddComponent.");
            }
            if (_components is null)
            {
                _components = new Dictionary<string, int>();
            }
            _components[c.GetType().Name] = (int)c.EID!;
            c._addToEntity(this);
        }

        protected override string? getBG()
        {
            if (HasComponent<Defender>())
            {
                int Wounds = GetComponent<Defender>().Wounds;
                if (Wounds >= 6)
                {
                    return "red";
                }
                else if (Wounds >= 4)
                {
                    return "orange";
                }
                else if (Wounds >= 2)
                {
                    return "yellow";
                }
            }
            return base.getBG();
        }
    }
}
