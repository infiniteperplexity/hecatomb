/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/5/2018
 * Time: 2:26 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb
{
	public class TurnHandler
	{
		public int Turn;
		[JsonIgnore] public List<Actor> Acted;
		[JsonIgnore] public List<Actor> Queue;
		
		public TurnHandler()
		{
			Turn = 0;
		}
		
		public void Try()
		{
			if (Game.World.Player.Acted)
			{
				Next();
			}
		}
		public void Next()
		{
			Player p = Game.World.Player;
			p.Acted = false;
			Game.GraphicsDirty = true;
			Turn+=1;
			Creature[] actors = Game.World.Creatures.ToArray();
			foreach (Creature m in p.GetMinions())
			{
				Minion minion = m.GetComponent<Minion>();
				if (minion.Task==null)
				{
					foreach (TaskEntity t in Game.World.Tasks)
					{
						if (t.GetComponent<Task>().Worker==null)
						{
							t.GetComponent<Task>().Worker = m;
							minion.Task = t;
							break;
						}
					}
				}
			}
			foreach (Creature cr in actors)
			{
				Actor actor = cr.TryComponent<Actor>();
				if (actor!=null)
				{
					actor.Act();
				}
			}
			p.HandleVisibility();
		}
			// check in on the daily cycle
			// publish a turn begin event
			// assign tasks as necessary
//			queue = [];
//		    deck = [];
//		    for (let cr of HTomb.World.things) {
//		      if (cr.actor && cr.unstaged===null) {
//		        cr.actor.regainPoints();
//		        queue.push(cr);
//		      }
//    		}
//		  	queue.sort(function(a,b) {
//		      if (a.actor.actionPoints < b.actor.actionPoints) {
//		        return -1;
//		      } else if (a.actor.actionPoints > b.actor.actionPoints) {
//		        return 1;
//		      } else if (a===HTomb.Player) {
//		        return -1;
//		      } else if (b===HTomb.Player) {
//		        return 1;
//		      } else if (a.spawnId < b.spawnId) {
//		        return -1;
//		      } else if (a.spawnId > b.spawnId) {
//		        return 1;
//		      } else {
//		        return 0;
//		      }
//		    });
// Begin recursive traversal of the queue
//    nextActor();
	}
}
