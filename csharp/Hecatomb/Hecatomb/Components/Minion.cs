/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/25/2018
 * Time: 12:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;

namespace Hecatomb
{
    using static HecatombAliases;
    public class Minion : Component
    {

        public TaskField Task = new TaskField();
        new string[] Required = new string[] {"Actor"};

        public Minion()
        {
            AddListener<ActEvent>(OnAct);
        }

        public GameEvent OnAct(GameEvent ge)
        {
            ActEvent ae = (ActEvent)ge;
            if (ae.Entity == Entity.Unbox())
            {
                if (!ae.Actor.Acted)
                    ae.Actor.Alert();

                if (!ae.Actor.Acted)
                    Act();

                if (!ae.Actor.Acted)
                    ae.Actor.Wander();
            }
            return ge;
        }
		
        public void Act()
        {
            var (x, y, z) = Entity;
            Creature cr = (Creature)Entity;
            if (Terrains[x, y, z].Solid && Task?.ClassName != "ZombieEmergeTask")
            {
                if (Features[x, y, z + 1]?.TypeName == "Grave")
                {
                    Tasks[x, y, z + 1]?.Cancel();
                    var task = Spawn<ZombieEmergeTask>();
                    task.Place(x, y, z + 1);
                    task.AssignTo(cr);
                }
            }
            if (Task==null)
            {
                foreach (Task task in Tasks.OrderBy(t=> t.Priority).ThenBy(t => Tiles.QuickDistance(x, y, z, t.X, t.Y, t.Z)).ToList())
                //    foreach (Task task in Tasks.OrderBy(t=>Tiles.QuickDistance(x, y, z, t.X, t.Y, t.Z)).ToList())
                {
                    if (task.Worker==null && task.CanAssign(cr))
                    {
                        task.AssignTo(cr);
                        break;
                    }
                }
            }
            if (Task!=null)
            {
                Task.Act();
            }
            //Task?.Act();
            Actor actor = Entity.GetComponent<Actor>();
            if (!actor.Acted)
            {
                var p = Game.World.Player;
                actor.Patrol(p.X, p.Y, p.Z);
            }         
        }
	}
}
