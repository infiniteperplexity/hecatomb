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

            int hardness = Game.World.Covers[x, y, z + 1].Hardness;
            if (Game.Options.IgnoreHardness || Game.World.GetState<ResearchHandler>().GetToolHardness() >= hardness)
            {
                Game.World.Covers[x, y, z + 1].Mine(x, y, z + 1);
                Terrains[x, y, z + 1] = Terrain.DownSlopeTile;
                Game.World.Events.Publish(new DigEvent() { X = x, Y = y, Z = z, EventType = "Ramp" });
            }
            else
            {
                if (Terrains[x, y, z + 1].Solid)
                {
                    Game.InfoPanel.PushMessage("A ramp was placed but the ceiling was too hard to dig into.");
                }
            }
            Game.World.ValidateOutdoors();
        }
    }
}
