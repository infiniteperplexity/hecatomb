using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    abstract class ComposedEntity : TileEntity
    {
        Dictionary<string, EntityField<Component>>? Components;

        public Component GetComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components is null || !Components.ContainsKey(t))
            {
                throw new InvalidOperationException($"{this} has no component of type {t}");
            }
            return Components![t].UnboxBriefly()!;
        }

        public bool HasComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components is null || !Components.ContainsKey(t))
            {
                return false;
            }
            return false;
        }

        public void AddComponent(Component c)
        {
            if (Components is null)
            {
                Components = new Dictionary<string, EntityField<Component>>();
            }
            Components[c.GetType().Name] = c;
        }
    }

}
