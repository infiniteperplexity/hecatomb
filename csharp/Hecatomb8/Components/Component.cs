using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    // A component is an entity, attached to a creature or feature, that provides a chunk of interrelated functionality, such as movement, decision-making, et cetera
    public class Component : Entity
    {
        public ListenerHandledEntityPointer<ComposedEntity>? Entity;

        protected Component()
        {
        }

        public void AddToEntity(ComposedEntity t)
        {
            Entity = t.GetPointer<ComposedEntity>(OnDespawn);
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
    }
}
