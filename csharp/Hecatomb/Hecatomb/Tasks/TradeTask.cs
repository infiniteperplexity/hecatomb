using System;
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
    public class TradeTask : Task
    {
        public TileEntityField<Structure> Structure;
        public Dictionary<string, int> Trading;

        public TradeTask() : base()
        {
            Priority = 3;
        }
        public override string GetDisplayName()
        {
            return $"trading for {Resource.Format(Trading) }";
        }
        public override ColoredText ListOnMenu()
        {
            // should never happen
            if (Ingredients.Count == 0)
            {
                return $"Trade for {Resource.Format(Trading)}";
            }
            else
            {
                bool available = false;
                if (Game.World.Player.GetComponent<Movement>().CanFindResources(Ingredients))
                {
                    available = true;
                }
                return (((available) ? "{white}" : "{gray}") + Resource.Format(Ingredients) + " for " + " ($: " + Resource.Format(Trading) + ")");
            }
        }

        public override void ChooseFromMenu()
        {
            if (Game.World.Player.GetComponent<Movement>().CanFindResources(Ingredients))
            {
                int x = Structure.X;
                int y = Structure.Y;
                int z = Structure.Z;
                // could I forceably spawn this rather than just copying it?
                TradeTask t = Entity.Spawn<TradeTask>();
                t.Structure = Structure;
                t.Ingredients = Ingredients;
                t.Trading = Trading;
                t.Labor = Labor;
                t.LaborCost = LaborCost;
                t.Place(x, y, z);
            }
            Controls.Set(new MenuChoiceControls(Structure.Unbox()));
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
            if (Labor < LaborCost)
            {
                return false;
            }
            return base.CanAssign(c);
        }
        public override void Work()
        {
            Labor -= (1 + Options.WorkBonus);
            Unassign();
            Game.World.Events.Subscribe<TurnBeginEvent>(this, OnTurnBegin);
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent t = (TurnBeginEvent)ge;
            if (Labor >= LaborCost)
            {
                return ge;
            }
            Labor -= (1 + Options.WorkBonus);
            if (Labor <= 0)
            {
                Finish();
            }
            return ge;
        }
        public override void Finish()
        {
            foreach (string resource in Trading.Keys)
            {
                int n = Trading[resource];
                Item.PlaceNewResource(resource, n, X, Y, Z);
            }
            Complete();
        }
    }
}
