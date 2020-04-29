using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Minion : Component
    {
        // should maybe make this private?  dangerous to assign directly to it
        public ListenerHandledEntityPointer<Task>? Task;

        public Minion()
        {
            //AddListener<ActEvent>(OnAct);
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            var de = (DespawnEvent)ge;
            if (Task != null && de.Entity == Task.UnboxBriefly())
            {
                Task = null;
            }
            base.OnDespawn(ge);
            return ge;
        }

        public GameEvent OnAct(GameEvent ge)
        {
            //ActEvent ae = (ActEvent)ge;
            //if (ae.Entity == Entity.Unbox() && ae.Step == "BeforeWander")
            //{
            //    if (!ae.Actor.Acted)
            //    {
            //        Act();
            //    }
            //}
            return ge;
        }

        public void Act()
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            Creature cr = (Creature)Entity.UnboxBriefly()!;
            var (x, y, z) = ((int)cr.X!, (int)cr.Y!, (int)cr.Z!);
            Task? existing = Task?.UnboxBriefly();
            // a zombie encased in solid rock, a grave on top, and no zombie emerge task...poor guy needs a zombie emerge task!
            if (Terrains.GetWithBoundsChecked(x, y, z).Solid && !(existing is ZombieEmergeTask) && Features.GetWithBoundsChecked(x, y, z + 1) is Grave)
            {
                Tasks.GetWithBoundsChecked(x, y, z + 1)?.Cancel();
                var t = Spawn<ZombieEmergeTask>();
                t.PlaceInValidEmptyTile(x, y, z + 1);
                t.AssignTo(cr);
            }
            existing = Task?.UnboxBriefly();
            // if we have no task, find a suitable one to assign
            if (existing is null)
            {
                // should be safely placed and non-null, if they're in the list in the first place
                foreach (Task task in Tasks.OrderBy(t => t.Priority).ThenBy(t => Tiles.Distance(x, y, z, (int)t.X!, (int)t.Y!, (int)t.Z!)).ToList())
                {
                    if (task.Worker?.UnboxBriefly() is null && task.CanAssign(cr))
                    {
                        task.AssignTo(cr);
                        break;
                    }
                }
            }
            if (Task?.UnboxBriefly() != null)
            {
                Task.UnboxBriefly()!.Act();
            }
            Actor actor = cr.GetComponent<Actor>();
            if (!actor.Acted)
            {
                var p = Player;
                actor.Patrol((int)p.X!, (int)p.Y!, (int)p.Z!);
            }
        }

        public override void Despawn()
        {
            if (Task?.UnboxBriefly() != null)
            {
                Task.UnboxBriefly()!.Unassign();
            }
            base.Despawn();
        }
    }
}
