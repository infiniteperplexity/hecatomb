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
            
            
            int x = Structure.X;
            int y = Structure.Y;
            int z = Structure.Z;
            CommandLogger.LogCommand(command: "ResearchTask", x: x, y: y, z: z, makes: Makes);
            Research research = Hecatomb.Research.Types[Makes];
            // could I forceably spawn this rather than just copying it?
            ResearchTask rt = Entity.Spawn<ResearchTask>();
            rt.Structure = Structure;
            rt.Makes = Makes;
            rt.Labor = LaborCost;
            rt.LaborCost = LaborCost;
            rt.Ingredients = new Dictionary<string, int>(Ingredients);
            rt.Place(x, y, z);
            if (!OldGame.ReconstructMode)
            {
                var c = new MenuChoiceControls(Structure.Unbox());
                c.SelectedMenuCommand = "Jobs";
                ControlContext.Set(c);
            }
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
            OldGame.World.Events.Subscribe<TurnBeginEvent>(this, OnTurnBegin);  
        }

        public override void Cancel()
        {
            Structure.Unbox().Researching = null;
            base.Cancel();
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
            var researched = OldGame.World.GetState<ResearchHandler>().Researched;
            if (!researched.Contains(Makes))
            {
                researched.Add(Makes);
            }
            OldGame.InfoPanel.PushMessage("{magenta}Research on " + Research.Types[Makes].Name + " complete!");
            if (Makes == "FlintTools")
            {
                OldGame.World.Events.Publish(new AchievementEvent() { Action = "ResearchFlintTools" });
            }
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
