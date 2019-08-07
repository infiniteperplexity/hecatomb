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

    public class HaulTask : Task
    {
        public TileEntityField<Structure> Structure;
        public string Resource;

        public HaulTask() : base()
        {
            MenuName = "stockpile goods";
            WorkRange = 0;
            Priority = 10;
            BG = "white";
            ShowIngredients = false;
        }

        public override string GetDisplayName()
        {
            if (Claims.Count==0)
            {
                // okay I think I had this logic worked out wrong but it's fixed now
                if (Worker == null)
                {
                    Debug.WriteLine("okay this is genuinely strange");
                    return "messed up haul task";
                }
                var carried = $"carried by {Worker.Describe()} at {Worker.X} {Worker.Y} {Worker.Z}";
                return $"haul {Resource} "+carried;
            }
            Item item = Entity.FromEID(Claims.Keys.ToList()[0]) as Item;
            if (item==null)
            {
                return $"haul {Resource}";
            }
            var (x, y, z) = item;
            string where = null;
            if (item.Placed)
            {
                where = $"from {x} {y} {z}";
            }
            return $"haul {item.Describe()} {where}";
        }

        public override bool ValidTile(Coord c)
        {
            if (Structure.Placed)
            {
                return true;
            }
            return false;
        }

        public override bool CanAssign(Creature c)
        {
            Coord crd = new Coord(X, Y, Z);
            if (!Explored.Contains(crd) && !Options.Explored)
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
            if (Claims.Count == 0)
            {
                Cancel();
                return false;
            }
            Item item = (Item)Entities[Claims.Keys.ToList()[0]];
            return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanReach(item);
        }

        public override bool NeedsIngredients()
        {
            return true;
        }

        // do we need to account for it if there is no item and no claims?
        public int HasSpace(string resource)
        {
            int size = Hecatomb.Resource.Types[resource].StackSize;
            Item item = Items[X, Y, Z];
            int stack = (item == null) ? 0 : item.Quantity;
            int claimed = 0;
            foreach(int value in Claims.Values)
            {
                claimed += value;
            }
            return size - (stack + claimed);
        }

        public override void ClaimIngredients()
        {
            // do nothing; ingredient claims for haul tasks are hardcoded
        }

        public override void Start()
        {
            Worker.GetComponent<Inventory>().Drop();
            Finish();
        }

        public override void SpendIngredient()
        {
            Ingredients.Clear();
        }

    }


}
