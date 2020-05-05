using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class HaulTask : Task
    {
        public ListenerHandledEntityHandle<Structure>? Structure;
        public Resource? Resource;

        public HaulTask() : base()
        {
            _name = "stockpile goods";
            WorkSameTile = true;
            Priority = 10;
            _bg = "white";
            ShowIngredients = false;
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            var de = (DespawnEvent)ge;
            if (de.Entity == Structure?.UnboxBriefly())
            {
                Cancel();
            }
            return base.OnDespawn(ge);
        }

        protected override string? getName()
        {
            if (Resource is null)
            {
                return _name;
            }
            if (Claims.Count == 0)
            {
                if (Worker == null)
                {
                    // so this can happen if the creature dies
                    return $"haul {Resource!.Name}";
                }
                return $"haul {Resource!.Name}";
            }
            Item? item = Entity.GetEntity<Item>(Claims.Keys.ToList()[0]);
            if (item == null)
            {
                return $"haul {Resource.Name}";
            }
            var (x, y, z) = item;
            string? where = null;
            if (item.Placed)
            {
                where = $"from {x} {y} {z}";
            }
            return $"haul {item.Describe()} {where}";
        }

        public override bool ValidTile(Coord c)
        {
            if (Structure?.UnboxBriefly() != null && Structure.UnboxBriefly()!.Placed)
            {
                return true;
            }
            return false;
        }

        public override bool CanAssign(Creature c)
        {
            if (!Placed || !Spawned || !c.Placed || !c.Spawned)
            {
                return false;
            }
            var (x, y, z) = GetValidCoordinate();
            Coord crd = new Coord(x, y, z);
            if (!Explored.Contains(crd) && !HecatombOptions.Explored)
            {
                return false;
            }
            if (!Placed)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                PushMessage("Canceling invalid task.");
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
            else if (!Entities.ContainsKey(Claims.Keys.ToList()[0]))
            {
                Cancel();
                return false;
            }
            Item item = (Item)Entities[Claims.Keys.ToList()[0]];
            return m.CanReachBounded(this, useLast: (WorkSameTile)) && m.CanReachBounded(item);
        }

        public override bool NeedsIngredients()
        {
            return true;
        }

        // what exactly is going on here?
        public int AvailableSpace(Resource resource)
        {
            if (!Spawned || !Placed)
            {
                return 0;
            }
            int size = resource.StackSize;
            Item? item = Items.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
            int stack = (item == null) ? 0 : item.N;
            int claimed = 0;
            foreach (int value in Claims.Values)
            {
                claimed += value;
            }
            return size - (stack + claimed);
        }

        public override void ClaimIngredients()
        {
            // do nothing; ingredient claims for haul tasks are hardcoded
        }

        public override void Work()
        {
            if (!Spawned || !Placed || Worker?.UnboxBriefly() is null)
            {
                return;
            }
            var worker = Worker.UnboxBriefly()!;
            if (!worker.Spawned || !worker.Placed)
            {
                return;
            }
            var (x, y, z) = worker.GetValidCoordinate();
            worker.GetComponent<Inventory>().Drop();
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f != null)
            {
                if (f is StructuralFeature)
                {
                    Structure? s = (f as StructuralFeature)!.Structure?.UnboxBriefly();
                    if (s != null && s.GetStored().Count >= 4)
                    {
                        Publish(new AchievementEvent() { Action = "FullyStocked" });
                    }
                }

            }
            Finish();
        }

        public override void SpendIngredient()
        {
            Ingredients.Clear();
        }

        public override void ValidateClaims()
        {
            //if (Options.HaulTaskClaims)
            //{
            //    base.ValidateClaims();
            //    return;
            //}
            int claims = Claims.Keys.Count;
            foreach (int eid in Claims.Keys.ToList())
            {
                // if it has despawned
                if (!Entities.ContainsKey(eid))
                {
                    Claims.Remove(eid);
                }
                else
                {
                    Item item = (Item)Entities[eid];
                    // if the item has been moved or disowned
                    if (!item.Placed || item.Disowned)
                    {
                        Claims.Remove(eid);
                    }
                }
            }
            // if the haulable item was moved, cancel the task
            if (Claims.Keys.Count < claims)
            {
                Cancel();
            }
        }

        public override void UnclaimIngredients()
        {
            //if (Options.HaulTaskClaims)
            //{
            //    base.UnclaimIngredients();
            //}
            //else
            //{
            Claims.Clear();
            //}
        }

        public override Item PickUpIngredient(int eid, Item item)
        {
            //if (Options.HaulTaskClaims)
            //{
            //    return base.PickUpIngredient(eid, item);
            //}
            //else
            //{
            return item.Take(Claims[eid]);
            //}
        }

        // haul tasks do not display their ingredients even when asked to
        public override ColoredText DescribeWithIngredients(bool capitalized = false, bool checkAvailable = false)
        {
            var name = getName()!;
            if (capitalized)
            {
                name = char.ToUpper(name[0]) + name.Substring(1);
            }
            return name;
        }
    }
}
