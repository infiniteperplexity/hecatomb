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
        }

        public override string GetDisplayName()
        {
            if (Claims.Count==0)
            {
                Debug.WriteLine("I'm not sure why you would try to display the name of a haul task with no claims");
                Debug.WriteLine($"The ingredient count is {Ingredients.Count}");
                return "what the heck?";
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
            else
            {
                where = $"carried by {Worker} at {Worker.X} {Worker.Y} {Worker.Z}";
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
