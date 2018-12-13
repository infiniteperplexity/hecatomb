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
	/// <summary>
	/// Description of Minion.
	/// </summary>
	public class Minion : Component
	{
	
		public int TaskEID;
		[JsonIgnore] public TaskEntity Task
		{
			get
			{
				if (TaskEID==-1)
				{
					return null;
				}
				else
				{
					return (TaskEntity) Entities[TaskEID];
				}
			}
			private set
			{
				if (value==null)
				{
					TaskEID = -1;
				}
				else
				{
					TaskEID = value.EID;
				}
			}
		}
		
		public Minion(): base()
		{
			TaskEID = -1;
			Required = new string[] {"Actor"};
		}
		
		// called by Task.AssignEntity
		public void _AssignTask(TaskEntity t)
		{
			Task = t;
		}
		
		public void _Unassign()
		{
			Task = null;
		}

        public void Act()
        {
            int x = Entity.X;
            int y = Entity.Y;
            int z = Entity.Z;
            Creature cr = (Creature)Entity;
            if ( Game.World.Terrains[x, y, z].Solid && Task?.TryComponent<Task>()?.ClassName != "ZombieEmergeTask")
            {
                if (Game.World.Features[x, y, z + 1]?.TypeName == "Grave")
                {
                    Game.World.Tasks[x, y, z + 1]?.TryComponent<Task>()?.Cancel();
                    var task = Hecatomb.Entity.Spawn<TaskEntity>("ZombieEmergeTask");
                    task.Place(x, y, z + 1);
                    task.GetComponent<Task>().AssignTo(cr);
                }
            }
            if (Task==null)
            {
                foreach(TaskEntity te in Game.World.Tasks.OrderBy(t=>Tiles.QuickDistance(x, y, z, t.X, t.Y, t.Z)).ToList())
                {
                    var task = te.GetComponent<Task>();
                    if (task.Worker==null && task.CanAssign(cr))
                    {
                        task.AssignTo(cr);
                        break;
                    }
                }
            }
            Task?.TryComponent<Task>()?.Act();
            Actor actor = Entity.GetComponent<Actor>();
            if (!actor.Acted)
            {
                var p = Game.World.Player;
                actor.Patrol(p.X, p.Y, p.Z);
            }         
        }
	}
}
