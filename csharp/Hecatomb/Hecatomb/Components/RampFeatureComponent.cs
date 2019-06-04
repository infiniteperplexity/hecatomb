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
            Game.World.Covers[x, y, z].Mine(x, y, z);
            Terrains[x, y, z] = Terrain.UpSlopeTile;

            int hardness = (Game.Options.IgnoreHardness) ? 0 : Game.World.Covers[x, y, z + 1].Hardness;
            if (hardness == 0 || Game.World.GetState<ResearchHandler>().Researched.Contains("FlintTools"))
            {
                Game.World.Covers[x, y, z + 1].Mine(x, y, z + 1);
                Terrains[x, y, z + 1] = Terrain.DownSlopeTile;
                Game.World.Events.Publish(new DigEvent() { X = x, Y = y, Z = z, EventType = "Ramp" });
            }
            Game.World.ValidateOutdoors();
        }
    }
}
