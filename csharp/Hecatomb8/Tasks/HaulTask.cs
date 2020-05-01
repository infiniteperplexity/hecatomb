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

    //public class HaulTask : Task
    //{
    //    public TileEntityField<Structure> Structure;
    //    public string Resource;

    //    public HaulTask() : base()
    //    {
    //        MenuName = "stockpile goods";
    //        WorkRange = 0;
    //        Priority = 10;
    //        BG = "white";
    //        ShowIngredients = false;
    //    }

    //    public override string GetDisplayName()
    //    {
    //        if (Claims.Count == 0)
    //        {
    //            if (Worker == null)
    //            {
    //                // so this can happen if the creature dies
    //                return $"haul {Hecatomb.Resource.Types[Resource].Name}";
    //            }
    //            var carried = $"carried by {Worker.Describe()} at {Worker.X} {Worker.Y} {Worker.Z}";
    //            return $"haul {Hecatomb.Resource.Types[Resource].Name}";
    //        }
    //        Item item = Entity.FromEID(Claims.Keys.ToList()[0]) as Item;
    //        if (item == null)
    //        {
    //            return $"haul {Hecatomb.Resource.Types[Resource].Name}";
    //        }
    //        var (x, y, z) = item;
    //        string where = null;
    //        if (item.Placed)
    //        {
    //            where = $"from {x} {y} {z}";
    //        }
    //        return $"haul {item.Describe()} {where}";
    //    }

    //    public override bool ValidTile(Coord c)
    //    {
    //        if (Structure.Placed)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    public override bool CanAssign(Creature c)
    //    {
    //        Coord crd = new Coord(X, Y, Z);
    //        if (!Explored.Contains(crd) && !Options.Explored)
    //        {
    //            return false;
    //        }
    //        if (!Placed)
    //        {
    //            return false;
    //        }
    //        if (!ValidTile(crd))
    //        {
    //            Status.PushMessage("Canceling invalid task.");
    //            Cancel();
    //            return false;
    //        }
    //        Movement m = c.GetComponent<Movement>();
    //        // if the Item has been removed...do something...
    //        if (Claims.Count == 0)
    //        {
    //            Cancel();
    //            return false;
    //        }
    //        else if (!Entities.ContainsKey(Claims.Keys.ToList()[0]))
    //        {
    //            Cancel();
    //            return false;
    //        }
    //        Item item = (Item)Entities[Claims.Keys.ToList()[0]];
    //        return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanReach(item);
    //    }

    //    public override bool NeedsIngredients()
    //    {
    //        return true;
    //    }

    //    // do we need to account for it if there is no item and no claims?
    //    public int HasSpace(string resource)
    //    {
    //        int size = Hecatomb.Resource.Types[resource].StackSize;
    //        Item item = Items[X, Y, Z];
    //        int stack = (item == null) ? 0 : item.Quantity;
    //        int claimed = 0;
    //        foreach (int value in Claims.Values)
    //        {
    //            claimed += value;
    //        }
    //        return size - (stack + claimed);
    //    }

    //    public override void ClaimIngredients()
    //    {
    //        // do nothing; ingredient claims for haul tasks are hardcoded
    //    }

    //    public override void Work()
    //    {
    //        var (x, y, z) = Worker;

    //        Worker.GetComponent<Inventory>().Drop();
    //        Feature f = Features[x, y, z];
    //        if (f != null)
    //        {
    //            if (f.TryComponent<StructuralComponent>() != null)
    //            {
    //                Structure s = f.GetComponent<StructuralComponent>().Structure;
    //                if (s.GetStored().Count >= 4)
    //                {
    //                    Game.World.Events.Publish(new AchievementEvent() { Action = "FullyStocked" });
    //                }
    //            }

    //        }
    //        Finish();
    //    }

    //    public override void SpendIngredient()
    //    {
    //        Ingredients.Clear();
    //    }

    //    public override void ValidateClaims()
    //    {
    //        if (Options.HaulTaskClaims)
    //        {
    //            base.ValidateClaims();
    //            return;
    //        }
    //        int claims = Claims.Keys.Count;
    //        foreach (int eid in Claims.Keys.ToList())
    //        {
    //            // if it has despawned
    //            if (!Entities.ContainsKey(eid))
    //            {
    //                Claims.Remove(eid);
    //            }
    //            else
    //            {
    //                Item item = (Item)Entities[eid];
    //                if (!item.Placed)
    //                {
    //                    Claims.Remove(eid);
    //                }
    //            }
    //        }
    //        // if the haulable item was moved, cancel the task
    //        if (Claims.Keys.Count < claims)
    //        {
    //            Cancel();
    //        }
    //    }

    //    public override void UnclaimIngredients()
    //    {
    //        if (Options.HaulTaskClaims)
    //        {
    //            base.UnclaimIngredients();
    //        }
    //        else
    //        {
    //            Claims.Clear();
    //        }
    //    }

    //    public override Item PickUpIngredient(int eid, Item item)
    //    {
    //        if (Options.HaulTaskClaims)
    //        {
    //            return base.PickUpIngredient(eid, item);
    //        }
    //        else
    //        {
    //            return item.Take(Claims[eid]);
    //        }
    //    }
    //}
}
