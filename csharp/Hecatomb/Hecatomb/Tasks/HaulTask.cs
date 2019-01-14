using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    class HaulTask : Task
    {
        public TileEntityField<Item> Item;
        public HaulTask() : base()
        {
            MenuName = "stockpile goods";
        }

        public override bool CanAssign(Creature c)
        {
            Coord crd = new Coord(X, Y, Z);
            if (!Explored.Contains(crd))
            {
                return false;
            }
            if (!Placed)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                Status.PushMessage("Canceling invalid task.");
                Cancel();
                return false;
            }
            Movement m = c.GetComponent<Movement>();
            // if the Item has been removed...do something...
            return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanReach(Item);
        }

        public override void AssignTo(Creature c)
        {
            c.GetComponent<Minion>().Task = this;
            Worker = c;
            Claims[Item.EID] = Ingredients;
        }

        public override void Start()
        {
            var inventory = Worker.GetComponent<Inventory>();
            inventory.DropResources(Ingredients);
            Finish();
        }

    }


}
