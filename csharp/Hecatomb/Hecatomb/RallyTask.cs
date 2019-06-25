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
    class RallyTask : Task
    {
        public RallyTask() : base()
        {
            MenuName = "set rally point";
            PrereqStructures = new List<string>() { "GuardPost" };
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
        }

        public override void SelectTile(Coord c)
        {
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null)
            {
                var rallies = Game.World.Tasks.Where((Task t) => t is RallyTask).ToList();
                foreach(var rally in rallies)
                {
                    rally.Cancel();
                }
                RallyTask task = Spawn<RallyTask>();
                task.Place(c.X, c.Y, c.Z);
                Game.World.Events.Subscribe<ActEvent>(task, task.OnAct);
                
            }
        }

        public GameEvent OnAct(GameEvent ge)
        {
            ActEvent ae = (ActEvent)ge;
            if (ae.Entity is Creature)
            {
                Creature cr = (Creature)ae.Entity;
                if (cr.TryComponent<Minion>()!=null)
                {
                    Debug.WriteLine($"Is this thing even spawned? {Spawned}");
                    Debug.WriteLine($"We shoudl be patrolling around {X} {Y} {Z}");
                    Actor actor = cr.GetComponent<Actor>();
                    actor.Patrol(X, Y, Z);
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
