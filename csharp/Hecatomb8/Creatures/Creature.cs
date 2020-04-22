using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    public class Creature : ComposedEntity
    {

        protected Creature()
        {
            Components.Add(new Movement());
            Components.Add(new Actor());
            Components.Add(new Senses());
        }
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Creatures.GetWithBoundsChecked(x, y, z) != null)
            {
                throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Creatures.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
            }
            Remove();
            GameState.World!.Creatures.SetWithBoundsChecked(x, y, z, this);
            base.PlaceInValidEmptyTile(x, y, z);
        }

        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Creatures.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
        }
    }
}
