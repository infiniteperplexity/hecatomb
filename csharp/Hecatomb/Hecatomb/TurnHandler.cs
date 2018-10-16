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
				Next();
			}
		}
		public void Next()
		{
			Player p = Game.World.Player;
			p.Acted = false;
			Turn+=1;
			Game.World.Events.Publish(new TurnBeginEvent() {Turn=Turn});
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
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
					Next(); // should call it NextTurn
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
			if (actor.Entity is Player)
			{
				// do visibility
				Game.World.Player.HandleVisibility();
				// recenter screen
				
				Game.MainPanel.Dirty = true;
				Game.MenuPanel.Dirty = true;
				Game.StatusPanel.Dirty = true;
				Game.World.Player.Acted = false;
				// set player acted to false
				return;
			}
			else
			{
				int checkPoints = actor.CurrentPoints;
				Debug.WriteLine(checkPoints);
				if (checkPoints>0)
				{
					actor.Act();
					if (actor.CurrentPoints==checkPoints)
					{
						throw new InvalidOperationException(String.Format("{0} somehow avoided using action poionts.", actor.Entity));
					}
				}
				if (actor.CurrentPoints>0)
				{
					Deck.Enqueue(actor);
				}
				NextActor();
			}
			// in english...if queue and deck are both 0, next turn
			// if queue is zero but deck is not, flip and next
			// otherwise just next
			
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
