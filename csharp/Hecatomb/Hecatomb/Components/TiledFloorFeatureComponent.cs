using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    public class TiledFloorFeatureComponent : Component
    {
        public override void AfterSelfPlace(int x, int y, int z)
        {
            Entity.Despawn();
            Covers[x, y, z] = Cover.TiledStone;
        }
    }
}
