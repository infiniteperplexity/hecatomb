using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    // A component is an entity, attached to a creature or feature, that provides a chunk of interrelated functionality, such as movement, decision-making, et cetera
    public class Component : Entity
    {
        public ListenerHandledEntityHandle<ComposedEntity>? Entity;

        protected Component()
        {
        }

        public void AddToEntity(ComposedEntity t)
        {
            Entity = t.GetHandle<ComposedEntity>(OnDespawn);
        }

        public virtual GameEvent OnDespawn(GameEvent ge)
        {
            if (ge is DespawnEvent)
            {
                var de = (DespawnEvent)ge;
                if (Entity != null && de.Entity == Entity.UnboxBriefly())
                {
                    Entity = null;
                }
            }
            return ge;
        }

        public void RemoveFromEntity()
        {
            if (Entity?.UnboxBriefly() is null)
            {
                return;
            }
            // if it's a plain old Component subclass, use its own type as the key
            if (this.GetType().BaseType == typeof(Component))
            {
                var components = Entity.UnboxBriefly()!._components;
                if (components != null && components.ContainsKey(this.GetType().Name))
                {
                    components.Remove(this.GetType().Name);
                }
            }
        }
    }
}
