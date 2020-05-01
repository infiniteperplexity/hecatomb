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
    public class ResearchTask : Task
    {
        public ListenerHandledEntityHandle<Structure>? Structure;
        public Research Researching;

        public ResearchTask() : base()
        {
            _name = "research";
            Priority = 3;
            _bg = "magenta";
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity! == Structure?.UnboxBriefly())
            {
                Despawn();
            }
            return base.OnDespawn(ge);
        }

        protected override string getName()
        {
            if (Makes is null)
            {
                return _name!;
            }
            return $"researching {Makes.Name}";
        }


        public override void ChooseFromMenu()
        {
            if (Structure?.UnboxBriefly() is null || !Structure.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x, y, z) = Structure.UnboxBriefly()!.GetVerifiedCoord();
            if (Tasks.GetWithBoundsChecked(x, y, z) != null)
            {
                return;
            }
            CommandLogger.LogCommand(command: "ResearchTask", x: x, y: y, z: z, makes: Makes!.Name);
            //Research research = Hecatomb.Research.Types[Makes];
            ResearchTask rt = Entity.Spawn<ResearchTask>();
            rt.Structure = Structure;
            rt.Makes = Makes;
            rt.Labor = LaborCost;
            rt.LaborCost = LaborCost;
            rt.Ingredients = new JsonArrayDictionary<Resource, int>(Ingredients);
            rt.PlaceInValidEmptyTile(x, y, z);
            //if (!Game.ReconstructMode)
            //{
            //    var c = new MenuChoiceControls(Structure.Unbox());
            //    c.SelectedMenuCommand = "Jobs";
            //    ControlContext.Set(c);
            //}
        }

        public override bool ValidTile(Coord c)
        {
            if (Structure?.UnboxBriefly() is null || !Structure.UnboxBriefly()!.Placed)
            {
                return false;
            }
            return true;
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

        public override void Cancel()
        {
            if (Structure?.UnboxBriefly() != null)
            {
                Structure.UnboxBriefly()!.ResarchTask = null;
            }
            base.Cancel();
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
        //public override void Finish()
        //{
        //    var researched = GetState<ResearchHandler>().Researched;
        //    if (!researched.Contains(Makes))
        //    {
        //        researched.Add(Makes);
        //    }
        //    PushMessage("{magenta}Research on " + Research.Types[Makes].Name + " complete!");
        //    if (Makes == "FlintTools")
        //    {
        //        Publish(new AchievementEvent() { Action = "ResearchFlintTools" });
        //    }
        //    Complete();
        //}

        //public override string GetHoverName()
        //{
        //    if (Ingredients.Count == 0 || Options.NoIngredients)
        //    {
        //        return Describe(article: false) + " (" + Labor + " turns)";
        //    }
        //    return (Describe(article: false) + " ($: " + Resource.Format(Ingredients) + ")");
        //}
    }
}
