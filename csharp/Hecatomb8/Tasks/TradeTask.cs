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
    public class TradeTask : Task
    {
        public ListenerHandledEntityHandle<Structure>? Structure;
        public Dictionary<Resource, int> Trading;

        public TradeTask() : base()
        {
            Priority = 3;
            Labor = 50;
            LaborCost = 50;
            Trading = new JsonArrayDictionary<Resource, int>();
            _bg = "#FFFF88";

        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            var de = (DespawnEvent)ge;
            if (de.Entity == Structure?.UnboxBriefly())
            {
                Cancel();
                Despawn();
            }
            return ge;
        }
        protected override string getName()
        {
            return $"trading for {Resource.Format(Trading) }";
        }

        public override bool NeedsIngredients()
        {
            return true;
        }


        public override ColoredText ListOnMenu()
        {
            if (Ingredients.Count == 0)
            {
                return $"Trade for {Resource.Format(Trading)}";
            }
            else
            {
                bool available = false;
                if (CanFindResources(Ingredients))
                {
                    available = true;
                }
                return (((available) ? "{white}" : "{gray}") + Resource.Format(Trading) + " for " + " ($: " + Resource.Format(Ingredients) + ")");
            }
        }

        public override void ChooseFromMenu()
        {
            if (Structure?.UnboxBriefly() is null || !Structure?.UnboxBriefly()!.Placed)
            {
                return;
            }
            var market = (Structure.UnboxBriefly() as BlackMarket)!;
            var menu = (InterfaceState.Controls as InfoDisplayControls);
            if (menu is null)
            {
                return;
            }
            // this...is a little crazy.  but it should work
            int? index = null;
            for (int i = 0; i < menu.Choices.Count; i++)
            {
                var choice = (menu.Choices[i] as TradeTask);
                if (choice is null)
                {
                    return;
                }
                if (this == choice)
                {
                    index = i;
                }
            }
            if (index is null)
            {
                return;
            }
            var (x, y, z) = market.GetVerifiedCoord();
            if (Tasks.GetWithBoundsChecked(x, y, z) != null)
            {
                return;
            }
            CommandLogger.LogCommand(command: "TradeTask", x: x, y: y, z: z, n: (int)index);
            if (Tasks.GetWithBoundsChecked(x, y, z) is null)
            {
                market.AvailableTradeIndexes.RemoveAt((int)index);
                PlaceInValidEmptyTile(x, y, z);
            }
            //if (!Game.ReconstructMode)
            //{
            //    var c = new MenuChoiceControls(Structure.Unbox());
            //    c.SelectedMenuCommand = "Jobs";
            //    c.MenuCommandsSelectable = false;
            //    ControlContext.Set(c);
            //}
        }

        public override bool ValidTile(Coord c)
        {
            if (Structure?.UnboxBriefly() != null && Structure.UnboxBriefly()!.Placed)
            {
                return true;
            }
            return false;
        }

        public override void Cancel()
        {
            if (Structure?.UnboxBriefly() != null)
            {
                (Structure.UnboxBriefly() as BlackMarket)!.Trading = null;
            }
            base.Cancel();
        }


        public override bool CanAssign(Creature c)
        {
            if (Labor < LaborCost)
            {
                return false;
            }
            return base.CanAssign(c);
        }
        public override void Work()
        {
            Labor -= (1 + HecatombOptions.WorkBonus);
            Unassign();
            Subscribe<TurnBeginEvent>(this, OnTurnBegin);
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent t = (TurnBeginEvent)ge;
            if (Labor >= LaborCost)
            {
                return ge;
            }
            Labor -= (1 + HecatombOptions.WorkBonus);
            if (Labor <= 0)
            {
                Finish();
            }
            return ge;
        }
        public override void Finish()
        {
            if (!Spawned || !Placed)
            {
                return;
            }
            foreach (Resource resource in Trading.Keys)
            {
                int n = Trading[resource];
                var item = Item.SpawnNewResource(resource, n);
                item.DropOnValidTile((int)X!, (int)Y!, (int)Z!);
            }
            Publish(new AchievementEvent() { Action = "FinishedTrade" });
            Complete();
        }
    }
}