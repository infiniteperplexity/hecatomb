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
    class RallyTask : Task
    {
        public RallyTask() : base()
        {
            _name = "rally all minions";
            RequiresStructures = new List<Type>() { typeof(GuardPost) };
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectTileControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            InterfaceState.SetControls(c);
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "RallyTask", x: c.X, y: c.Y, z: c.Z);
            if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null)
            {
                var rallies = Tasks.Where((Task t) => t is RallyTask).ToList();
                foreach (var rally in rallies)
                {
                    rally.Cancel();
                }
                RallyTask task = Spawn<RallyTask>();
                task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                Subscribe<ActEvent>(task, task.OnAct);

            }
        }

        // wait...why do we do it this way?  there must be some reason
        public GameEvent OnAct(GameEvent ge)
        {
            if (!Placed || !Spawned)
            {
                return ge;
            }
            ActEvent ae = (ActEvent)ge;
            if (ae.Entity is Creature && ae.Step == "BeforeAlert")
            {
                // we need some kind of sensible priority system here, to avoid conflicts between this and Minion.OnAct
                Creature cr = (Creature)ae.Entity;
                if (cr.HasComponent<Minion>())
                {
                    Actor actor = cr.GetComponent<Actor>();
                    actor.Patrol((int)X!, (int)Y!, (int)Z!);
                }
            }
            return ge;
        }
        public override bool CanAssign(Creature c)
        {
            return false;
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
    }
}
