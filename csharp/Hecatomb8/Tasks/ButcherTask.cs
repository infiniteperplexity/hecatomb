using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    class ButcherTask : Task
    {
        public ButcherTask() : base()
        {
            _name = "butcher corpse";
            WorkSameTile = true;
            Priority = 4;
            LaborCost = 5;
            Labor = LaborCost;
            Ingredients[Resource.Corpse] = 1;
            RequiresStructures = new List<Type>() { typeof(Slaughterhouse) };
            _bg = "pink";
            ShowIngredients = false;
        }

        protected override string getName()
        {
            if (Claims.Count == 0)
            {
                return _name!;
            }
            Item? item = Entity.GetEntity<Item>(Claims.Keys.ToList()[0]);
            if (item is null || !item.Placed || !item.Spawned)
            {
                return _name!;
            }
            return $"butcher {item.Describe()}";
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (ValidTile(c))
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Butcher corpse at {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Can't butcher corpse here." };
            }
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            Item? corpse = Items.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (corpse != null && Claims.ContainsKey((int)corpse.EID!))
            {
                return true;
            }
            // we are awkwardly using "validTile" for two different things which I'm sure will cause problems
            else if (corpse != null && corpse is Corpse && corpse.Claimed == 0 && Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) != this)
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
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectZoneControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Butcher corpse."};
            InterfaceState.SetControls(c);
        }

        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "ButcherTask", squares: squares);
            foreach (var c in squares)
            {
                Item? corpse = Items.GetWithBoundsChecked(c.X, c.Y, c.Z);
                if (!(corpse is Corpse))
                {
                    continue;
                }
                if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) == null)
                {
                    ButcherTask task = Spawn<ButcherTask>();
                    task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    task.Claims[(int)corpse!.EID!] = 1;
                    corpse.Claimed = 1;
                }
            }
        }



        public override bool CanAssign(Creature c)
        {
            if (!Spawned || !Placed)
            {
                return false;
            }
            Coord crd = GetPlacedCoordinate();
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
            if (!c.Placed || !c.Spawned)
            {
                return false;
            }
            Movement m = c.GetComponent<Movement>();
            if (Claims.Keys.Count == 0)
            {
                return false;
            }
            Item item = (Item)Entities[Claims.Keys.ToList()[0]];
            return m.CanReachBounded(this, useLast: (WorkSameTile)) && m.CanReachBounded(item);
        }

        public override bool NeedsIngredients()
        {
            return true;
        }

        public override void ClaimIngredients()
        {

        }

        public override void Start()
        {
            if (!Placed || !Spawned || Worker?.UnboxBriefly() is null)
            {
                return;
            }
            Worker.UnboxBriefly()!.GetComponent<Inventory>().Drop();
        }

        public override void Finish()
        {
            if (!Placed || !Spawned || Worker?.UnboxBriefly() is null)
            {
                return;
            }
            var (x, y, z) = GetPlacedCoordinate();
            Item? corpse = Items.GetWithBoundsChecked(x, y, z);
            if (corpse is null)
            {
                Cancel();
                return;
            }
            if (corpse is Corpse)
            {
                Publish(new AchievementEvent() { Action = "ButcherCorpse" });
                var flesh = Item.SpawnNewResource(Resource.Flesh, 1);
                var bone = Item.SpawnNewResource(Resource.Bone, 1);
                flesh.DropOnValidTile(x, y, z);
                bone.DropOnValidTile(x, y, z);
                base.Finish();
                corpse.Despawn();
            }
            else
            {
                Cancel();
            }
        }
        public override void Act()
        {
            if (!Placed || !Spawned || Worker?.UnboxBriefly() is null)
            {
                return;
            }
            if (CanWork())
            {
                Ingredients.Clear();
                Work();
            }
            else
            {
                Worker.UnboxBriefly()!.GetComponent<Actor>().WalkToward(this, useLast: (WorkSameTile));
            }
        }

    }
}
