/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:50 PM
 */using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class HarvestTask : Task
    {
        public HarvestTask() : base()
        {
            MenuDescription = "dig or harvest";
            _bg = "orange";
        }

        protected override string getName()
        {
            if (!Placed || !Spawned)
            {
                return MenuDescription;
            }
            var (x, y, z) = GetVerifiedCoord();
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f is null)
            {
                return MenuDescription;
            }
            if (f.HasComponent<Harvestable>())
            {
                return $"harvest {f.Name}";
            }
            else if (f is IncompleteFixture)
            {
                return $"remove {f.Name}";
            }
            else if (f.HasComponent<Fixture>())
            {
                return $"remove {f.Name}";
            }
            else if (f is StructuralFeature)
            {
                return $"remove {f.Name}";
            }
            return "an orphaned harvest task...wtf...";
        }

        public override bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            if (Features.GetWithBoundsChecked(x, y, z) is null)
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
            if (!Spawned || !Placed)
            {
                Despawn();
            }
            var (x, y, z) = GetVerifiedCoord();
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f != null && f.HasComponent<Harvestable>())
            {
                f.GetComponent<Harvestable>().Harvest();
                // it would be more graceful to do this with inheritance or event listeners
                if (f is Grave)
                {
                    // basically, if there's an explored tunnel underneath, it's more convenient if we don't dig a hole
                    if (Terrains.GetWithBoundsChecked(x, y, z - 1) == Terrain.WallTile || !Explored.Contains(new Coord(x, y, z)))
                    {
                        Terrains.SetWithBoundsChecked(x, y, z, Terrain.DownSlopeTile);
                        Terrains.SetWithBoundsChecked(x, y, z - 1, Terrain.UpSlopeTile);
                        Cover.ClearGroundCover(x, y, z);
                        Cover.Mine(x, y, z - 1);
                        //Game.World.ValidateOutdoors();
                    }
                }
            }
            else
            {
                f?.Destroy();
            }
            base.Finish();
        }
    }

}
