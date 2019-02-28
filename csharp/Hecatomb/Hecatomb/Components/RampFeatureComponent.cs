using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    public class RampFeatureComponent : Component
    {
        public override void AfterSelfPlace(int x, int y, int z)
        {
            Entity.Despawn();
            Covers[x, y, z].Mine(x, y, z);
            Terrains[x, y, z] = Terrain.UpSlopeTile;
            Covers[x, y, z + 1].Mine(x, y, z + 1);
            Terrains[x, y, z + 1] = Terrain.DownSlopeTile;
        }
    }
}
