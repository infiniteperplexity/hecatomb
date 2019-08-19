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
    public class ResearchTask : Task
    {
        public TileEntityField<Structure> Structure;

        public ResearchTask() : base()
        {
            Priority = 3;
            BG = "magenta";
        }
        public override string GetDisplayName()
        {
            return $"researching {Research.Types[Makes].Name}";
        }


        public override void ChooseFromMenu()
        {
            Research research = Hecatomb.Research.Types[Makes];
            //if (Game.World.Player.GetComponent<Movement>().CanFindResources(research.Ingredients))
            //{
                int x = Structure.X;
                int y = Structure.Y;
                int z = Structure.Z;
                // could I forceably spawn this rather than just copying it?
                ResearchTask rt = Entity.Spawn<ResearchTask>();
                rt.Structure = Structure;
                rt.Makes = Makes;
                rt.Labor = LaborCost;
                rt.LaborCost = LaborCost;
                rt.Ingredients = new Dictionary<string, int>(Ingredients);
                rt.Place(x, y, z);
            //}
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
            Game.StatusPanel.PushMessage("{blue}Research on " + Describe(article: false) + " completed!");
            Complete();
        }

        public override string GetHoverName()
        {
            if (Ingredients.Count == 0 || Options.NoIngredients)
            {
                return Describe(article: false) + " (" + Labor + " turns)";
            }
            return (Describe(article: false) + " ($: " + Resource.Format(Ingredients) + ")");
        }
    }
}
