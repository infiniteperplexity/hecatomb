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

        public TaskField Task;
		
		public Minion(): base()
		{
            Task = new TaskField();
			Required = new string[] {"Actor"};
		}
		
		// called by Task.AssignEntity
		public void _AssignTask(Task t)
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
            if ( Game.World.Terrains[x, y, z].Solid && Task?.ClassName != "ZombieEmergeTask")
            {
                if (Game.World.Features[x, y, z + 1]?.TypeName == "Grave")
                {
                    Game.World.Tasks[x, y, z + 1]?.Cancel();
                    var task = Spawn<ZombieEmergeTask>();
                    task.Place(x, y, z + 1);
                    task.AssignTo(cr);
                }
            }
            if (Task==null)
            {
                foreach(Task task in Game.World.Tasks.OrderBy(t=>Tiles.QuickDistance(x, y, z, t.X, t.Y, t.Z)).ToList())
                {
                    Debug.WriteLine("is null?");

                    Debug.WriteLine(task.Worker.EID);
                    Debug.WriteLine(task.Worker==null);
                    Debug.WriteLine("can assign?");
                    Debug.WriteLine(task.CanAssign(cr));
        
                    if (task.Worker==null && task.CanAssign(cr))
                    {
                        Debug.WriteLine("right here");
                        task.AssignTo(cr);
                        break;
                    }
                }
            }
            Task?.Act();
            Actor actor = Entity.GetComponent<Actor>();
            if (!actor.Acted)
            {
                var p = Game.World.Player;
                actor.Patrol(p.X, p.Y, p.Z);
            }         
        }
	}
}
