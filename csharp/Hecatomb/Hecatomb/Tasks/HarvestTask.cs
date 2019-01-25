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
        }

        public override string GetDisplayName()
        {
            Feature f = Game.World.Features[X, Y, Z];
            return $"harvest {f.Name}";
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
            }
            else
            {
                f.Remove();
            }
            base.Finish();
        }
    }

}
