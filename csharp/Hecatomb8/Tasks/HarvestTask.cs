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
            BG = "orange";
        }

        public override string GetDisplayName()
        {
            Feature f = OldGame.World.Features[X, Y, Z];
            if (f?.TryComponent<Harvestable>() != null)
            {
                return $"harvest {f.Name}";
            }
            else if (f?.TypeName=="IncompleteFeature")
            {
                return $"remove {f.Name}";
            }
            else if (f?.TryComponent<Fixture>()!=null)
            {
                return $"remove {f.Name}";
            }
            else if (f?.TryComponent<StructuralComponent>() != null)
            {
                return $"remove {f.Name}";
            }
            return "an orphaned harvest task...wtf...";
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            if (Features[c] == null)
            {
                return false;
            }
            return true;
            //if (Terrains[c.X, c.Y, c.Z] == Terrain.FloorTile)
            //{
            //    return true;
            //}
            //return false;
        }

        public override void Start()
        {
        }
        public override void Finish()
        {
            Feature f = OldGame.World.Features[X, Y, Z];
            Harvestable h = f.TryComponent<Harvestable>();
            if (h!=null)
            {
                

                h.Harvest();
                // there is some special handling for harvesting graves
                if (f.TypeName=="Grave")
                {
                    // basically, if there's an explored tunnel underneath, it's more convenient if we don't dig a hole
                    if (OldGame.World.Terrains[X, Y, Z - 1] == Terrain.WallTile || !OldGame.World.Explored.Contains(new Coord(X, Y, Z)))
                    {
                        OldGame.World.Terrains[X, Y, Z] = Terrain.DownSlopeTile;
                        OldGame.World.Terrains[X, Y, Z - 1] = Terrain.UpSlopeTile;
                        Cover.ClearCover(X, Y, Z);
                        Cover.ClearCover(X, Y, Z - 1);
                        OldGame.World.ValidateOutdoors();
                    }
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
