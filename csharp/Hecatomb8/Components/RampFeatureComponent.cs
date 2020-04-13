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
            OldGame.World.Covers[x, y, z].Mine(x, y, z);
            Terrains[x, y, z] = Terrain.UpSlopeTile;

            int hardness = OldGame.World.Covers[x, y, z + 1].Hardness;
            if (OldGame.Options.IgnoreHardness || OldGame.World.GetState<ResearchHandler>().GetToolHardness() >= hardness)
            {
                OldGame.World.Covers[x, y, z + 1].Mine(x, y, z + 1);
                Terrains[x, y, z + 1] = Terrain.DownSlopeTile;
                OldGame.World.Events.Publish(new DigEvent() { X = x, Y = y, Z = z, EventType = "Ramp" });
            }
            else
            {
                if (Terrains[x, y, z + 1].Solid)
                {
                    OldGame.InfoPanel.PushMessage("A ramp was placed but the ceiling was too hard to dig into.");
                }
            }
            OldGame.World.ValidateOutdoors();
        }
    }
}
