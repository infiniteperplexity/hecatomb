using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    class Creature : ComposedEntity
    {
        public override void PlaceInEmptyTile(Constrained<int> x, Constrained<int> y, Constrained<int> z)
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Creatures[new Constrained<int>((int)_x!), new Constrained<int>((int)_y!), new Constrained<int>((int)_z!)] = null;
            }
            GameState.World!.Creatures[x, y, z] = this;
        }
    }
}
