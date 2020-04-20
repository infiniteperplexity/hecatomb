using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    // A component is an entity, attached to a creature or feature, that provides a chunk of interrelated functionality, such as movement, decision-making, et cetera
    public class Component : Entity
    {
        public EntityField<TileEntity> Entity;

        protected Component()
        {
            Entity = new EntityField<TileEntity>();
        }

        public void AddToEntity(ComposedEntity t)
        {
            Entity = t;
        }
    }
}
