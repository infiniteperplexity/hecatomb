using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hecatomb
{

    public class RampFeatureComponent : Component
    {
        public override void AfterSelfPlace(int x, int y, int z)
        {
            Entity.Despawn();
            Game.World.Covers[x, y, z].Mine(x, y, z);
            Game.World.Terrains[x, y, z] = Terrain.UpSlopeTile;
            Game.World.Covers[x, y, z + 1].Mine(x, y, z + 1);
            Game.World.Terrains[x, y, z + 1] = Terrain.DownSlopeTile;
        }
    }
}
