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
        public HaulTask() : base()
        {
            MenuName = "stockpile goods";
            WorkRange = 0;
        }

        public override string GetDisplayName()
        {

            Item item = (Item) Entity.Entities[Claims.Keys.ToList()[0]];
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

        public override void ChooseFromMenu()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseDigTask" });
            //			Game.Controls.Set(new SelectTileControls(this));
            Game.Controls.Set(new SelectZoneControls(this));
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
            int size = Resource.Types[resource].StackSize;
            Item item = Items[X, Y, Z];
            int stack = (item == null) ? 0 : item.Quantity;
            int claimed = 0;
            foreach(int value in Claims.Values)
            {
                claimed += value;
            }
            return size - (stack + claimed);
        }
        public override void AssignTo(Creature c)
        {
            c.GetComponent<Minion>().Task = this;
            Worker = c;
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
