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
		// already acted, but still have points to act again
		[JsonIgnore] public Queue<Actor> Deck;
		// have not yet acted and have points remaining
		[JsonIgnore] public Queue<Actor> Queue;
		
		public TurnHandler()
		{
			Turn = 0;
			Deck = new Queue<Actor>();
			Queue = new Queue<Actor>();
		}
		
		public void Try()
		{
			if (Game.World.Player.Acted)
			{
				NextTurn();
			}
		}
		public void NextTurn()
		{
			
			Player p = Game.World.Player;
			p.Acted = false;
			Turn+=1;
            Game.World.Events.Publish(new TutorialEvent() { Action = "TurnBegin" });
            Game.World.Events.Publish(new TurnBeginEvent() {Turn=Turn});
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
			Creature[] actors = Game.World.Creatures.ToArray();
			foreach (Creature m in p.GetComponent<Minions>())
			{
				Minion minion = m.GetComponent<Minion>();
				if (minion.Task==null)
				{
                    // special case: if the initial ZombieEmerge task got canceled
                    int x = m.X;
                    int y = m.Y;
                    int z = m.Z;
                    if (Game.World.Tiles[x, y, z].Solid)
                    {
                        if (Game.World.Features[x, y, z+1]?.TypeName=="Grave")
                        {
                            Game.World.Tasks[x, y, z + 1]?.TryComponent<Task>()?.Cancel();
                            var task = Game.World.Entities.Spawn<TaskEntity>("ZombieEmergeTask");
                            task.Place(x, y, z + 1);
                            task.GetComponent<Task>().AssignTo(m);               
                        }
                    }
					foreach (TaskEntity t in Game.World.Tasks)
					{
						
						Task task = t.GetComponent<Task>();
						if (task.Worker==null)
						{
							
							if (task.CanAssign(m))
							{
								task.AssignTo(m);
								break;
							}
						}
					}
				}
			}
			// ***
			Queue.Clear();
			Deck.Clear();
			foreach (GameEntity e in Game.World.Entities.Spawned.Values)
			{
				if (e is Actor)
				{
					Actor actor = (Actor) e;
					actor.Regain();
					Queue.Enqueue(actor);
				}
			}
			Queue.OrderBy(a=>a.CurrentPoints).ThenBy(a=>a.EID);
			NextActor();
		}
		
		public void NextActor()
		{	
			// maybe not a real method
			if (Queue.Count==0)
			{
				if (Deck.Count==0)
				{
					Game.World.Events.Publish(new TurnEndEvent() {Turn=Turn});
					// publish turn end event
					NextTurn();
					return;
				}
				else
				{
					while (Deck.Count>0)
					{
						Queue.Enqueue(Deck.Dequeue());
					}
					Queue.OrderBy(a=>a.CurrentPoints).ThenBy(a=>a.EID);
				}
			}
			Actor actor = Queue.Dequeue();
            // need to check if it's been despawned
            if (!actor.Spawned)
            {
                NextActor();
                return;
            }
			if (actor.Entity is Player)
			{
				Game.World.Player.Ready();
				// set player acted to false
				return;
			}
			else
			{
				int checkPoints = actor.CurrentPoints;
				if (checkPoints>0)
				{
					actor.Act();
					if (actor.CurrentPoints==checkPoints)
					{
						throw new InvalidOperationException(String.Format("{0} somehow avoided using action points.", actor.Entity));
					}
				}
				if (actor.CurrentPoints>0)
				{
                    actor.Acted = false;
					Deck.Enqueue(actor);
				}
				NextActor();
			}
		}
		
		public void PlayerActed()
		{
			//Game.Time.Acted();
			Actor actor = Game.World.Player.GetComponent<Actor>();
			if (actor.CurrentPoints>0)
			{
				Deck.Enqueue(actor);
			}
			// bunch of time and interface-handling stuff
			NextActor();
		}
		
		public Queue<int> QueueAsIDs(Queue<Actor> q)
		{
			List<Actor> list = q.ToList();
			Queue<int> qi = new Queue<int>();
			for (int i=0; i<list.Count; i++)
			{
				qi.Enqueue(list[i].EID);
			}
			return qi;
		}
		
		public Queue<Actor> QueueAsActors(Queue<int> q)
		{
			List<int> list = q.ToList();
			Queue<Actor> qa = new Queue<Actor>();
			for (int i=0; i<list.Count; i++)
			{
				qa.Enqueue((Actor) Game.World.Entities.Spawned[list[i]]);
			}
			return qa;
		}
	}
}
