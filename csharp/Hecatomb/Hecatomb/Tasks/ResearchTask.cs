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
    class ResearchTask : Task
    {
        public TileEntityField<Structure> Structure;

        public override string GetDisplayName()
        {
            return $"researching {Research.Types[Makes].Name}";
        }
        public override ColoredText ListOnMenu()
        {
            Research research = Hecatomb.Research.Types[Makes];
            //you'd want to check for a path between the structure and the ingredients
            if (research.Ingredients.Count == 0)
            {
                return research.Name;
            }
            else
            {
                bool available = false;
                if (Game.World.Player.GetComponent<Movement>().CanFindResources(research.Ingredients))
                {
                    available = true;
                }
                return (((available) ? "{white}" : "{gray}") + research.Name + " ($: " + Resource.Format(research.Ingredients) + ")");
            }
        }

        public override void ChooseFromMenu()
        {
            Research research = Hecatomb.Research.Types[Makes];
            if (Game.World.Player.GetComponent<Movement>().CanFindResources(research.Ingredients))
            {
                int x = Structure.X;
                int y = Structure.Y;
                int z = Structure.Z;
                // could I forceably spawn this rather than just copying it?
                ResearchTask rt = Entity.Spawn<ResearchTask>();
                rt.Makes = Makes;
                rt.Labor = LaborCost;
                rt.LaborCost = LaborCost;
                rt.Ingredients = Ingredients;
                rt.Place(x, y, z);
            }
            Game.MenuPanel.Dirty = true;
        }

        public override bool CanAssign(Creature c)
        {
            if (Labor<LaborCost)
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
            if (Labor>=LaborCost)
            {
                return ge;
            }
            Labor -= (1+Options.WorkBonus);
            if (Labor<=0)
            {
                Finish();
            }
            return ge;
        }
        public override void Finish()
        {
            var researched = Game.World.GetState<ResearchHandler>().Researched;
            if (!researched.Contains(Makes))
            {
                researched.Add(Makes);
            }
            Complete();
        }
    }
}
