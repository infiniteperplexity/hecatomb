﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    using static HecatombAliases;
    class ButcherTask : Task
    {
        public ButcherTask() : base()
        {
            MenuName = "butcher corpse";
            WorkRange = 0;
            Priority = 4;
            Ingredients["Corpse"] = 1;
            PrereqStructures = new List<string>() { "Slaughterhouse" };
        }

        public override string GetDisplayName()
        {
            Item item = (Item)Entity.Entities[Claims.Keys.ToList()[0]];
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
            return $"butcher {item.Describe()} {where}";
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (ValidTile(c))
            {
                co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Butcher corpse at {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange}Can't butcher corpse here." };
            }
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            Item corpse = Items[c];
            if (corpse != null && Claims.ContainsKey(corpse.EID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
        }

        public override void SelectTile(Coord c)
        {
            
            // i think validity has been guaranteed at this point?
            
            var structures = Structure.ListStructures().Where(s=>s is Slaughterhouse).ToList();
            var (x, y, z) = structures[0];
            ButcherTask task = Spawn<ButcherTask>();
            task.Place(c.X, c.Y, c.Z);
            Item corpse = Items[c];
            task.Claims[corpse.EID] = 1;
            corpse.Claimed = 1;

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

        public override void AssignTo(Creature c)
        {
            c.GetComponent<Minion>().Task = this;
            Worker = c;
        }

        public override void Start()
        {
            Worker.GetComponent<Inventory>().Drop();
        }

        public override void Finish()
        {
            Item corpse = Items[X, Y, Z];
            if (corpse.Resource == "Corpse")
            {
                corpse.Despawn();
                Item.PlaceNewResource("Flesh", 1, X, Y, Z);
                Item.PlaceNewResource("Bone", 1, X, Y, Z);
                base.Finish();
            }
            else
            {
                Cancel();
            }
        }

        public override void SpendIngredient()
        {
            Ingredients.Clear();
        }

    }
}