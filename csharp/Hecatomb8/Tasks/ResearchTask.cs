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
            AddListener<TurnBeginEvent>(OnTurnBegin);
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
            if (Researching is null)
            {
                return _name!;
            }
            return $"research {Researching.Name}";
        }


        public override void ChooseFromMenu()
        {
            if (Structure?.UnboxBriefly() is null || !Structure.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x, y, z) = Structure.UnboxBriefly()!.GetValidCoordinate();
            if (Tasks.GetWithBoundsChecked(x, y, z) != null)
            {
                return;
            }
            CommandLogger.LogCommand(command: "ResearchTask", x: x, y: y, z: z, makes: Researching!.Name);
            //Research research = Hecatomb.Research.Types[Makes];
            ResearchTask rt = Entity.Spawn<ResearchTask>();
            rt.Structure = Structure;
            rt.Researching = Researching;
            rt.Labor = LaborCost;
            rt.LaborCost = LaborCost;
            rt.Ingredients = new JsonArrayDictionary<Resource, int>(Ingredients);
            rt.PlaceInValidEmptyTile(x, y, z);
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
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
                Structure.UnboxBriefly()!.ResearchTask = null;
            }
            base.Cancel();
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent t = (TurnBeginEvent)ge;
            if (Labor < LaborCost)
            {
                Labor -= (1 + HecatombOptions.WorkBonus);
                if (Labor <= 0)
                {
                    Finish();
                }
            }
            return ge;
        }
        public override void Finish()
        {
            var researched = GetState<ResearchHandler>().Researched;
            if (!researched.Contains(Researching))
            {
                researched.Add(Researching);
            }
            PushMessage("{magenta}Research on " + Researching.Name + " complete!");
            if (Researching == Research.FlintTools)
            {
                Publish(new AchievementEvent() { Action = "ResearchFlintTools" });
            }
            Complete();
        }

        public override ColoredText DescribeWithIngredients(bool capitalized = false, bool checkAvailable = false)
        {
            if (Ingredients.Count > 0)
            {
                return base.DescribeWithIngredients(capitalized: capitalized, checkAvailable: checkAvailable);
            }
            else
            {
                return base.DescribeWithIngredients(capitalized: capitalized, checkAvailable: checkAvailable) + " (" + Labor + " turns.)";
            }
        }
    }
}
