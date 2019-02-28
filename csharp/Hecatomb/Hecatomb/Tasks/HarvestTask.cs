/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    public class HarvestTask : Task
    {
        public HarvestTask() : base()
        {
            MenuName = "dig or harvest";
        }

        public override string GetDisplayName()
        {
            Feature f = Game.World.Features[X, Y, Z];
            return $"harvest {f.Name}";
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            if (Terrains[c.X, c.Y, c.Z] == Terrain.FloorTile)
            {
                return true;
            }
            return false;
        }

        public override void Start()
        {
        }
        public override void Finish()
        {
            Feature f = Game.World.Features[X, Y, Z];
            Harvestable h = f.TryComponent<Harvestable>();
            if (h!=null)
            {
                

                h.Harvest();
                if (f.TypeName=="Grave")
                {
                    Game.World.Terrains[X, Y, Z] = Terrain.DownSlopeTile;
                    Game.World.Terrains[X, Y, Z - 1] = Terrain.UpSlopeTile;
                    Game.World.Covers[X, Y, Z] = Cover.NoCover;
                    Game.World.Covers[X, Y, Z - 1] = Cover.NoCover;
                    Game.World.ValidateOutdoors();
                }
            }
            else
            {
                f.Remove();
            }
            base.Finish();
        }
    }

}
