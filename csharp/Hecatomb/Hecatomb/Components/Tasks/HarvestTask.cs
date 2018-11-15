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
    public class HarvestTask : Task
    {
        public HarvestTask() : base()
        {
            MenuName = "dig or harvest";
            TypeName = "harvest";
        }

        public override void Start()
        {
        }
        public override void Finish()
        {
            int x = Entity.X;
            int y = Entity.Y;
            int z = Entity.Z;
            Feature f = Game.World.Features[x, y, z];
            Harvestable h = f.TryComponent<Harvestable>();
            if (h!=null)
            {
                h.Harvest();
            }
            else
            {
                f.Remove();
            }
            base.Finish();
        }
    }

}
