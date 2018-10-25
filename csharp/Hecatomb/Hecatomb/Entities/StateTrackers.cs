/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/24/2018
 * Time: 10:09 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of StateTrackers.
	/// </summary>
	public class StateTracker: GameEntity
	{
		public virtual void Activate()
		{
			if (Game.World.StateTrackers.ContainsKey(GetType().Name))
			{
				Game.World.StateTrackers[GetType().Name].Despawn();
			}
			Game.World.StateTrackers[GetType().Name] = this;
		}
	}
	
	public class PathTracker: StateTracker
	{
		// cacheing misses is much more important than cacheing successes
		public Dictionary<int, Dictionary<int, int>> PathMisses;

		public override void Activate()
		{
			PathMisses = new Dictionary<int, Dictionary<int, int>>();
			Game.World.Events.Subscribe<TurnBeginEvent>(this, OnTurnBegin);
			base.Activate();
		}
		
		public void Reset()
		{
			PathMisses.Clear();
		}
	
		public GameEvent OnTurnBegin(GameEvent g)
		{
			foreach (int eid1 in PathMisses.Keys)
			{
				var dict = PathMisses[eid1];
				foreach (int eid2 in dict.Keys)
				{
					int n = dict[eid2];
					n-=1;
					if (n<=0)
					{
						dict.Remove(eid2);
						if (dict.Count==0)
						{
							PathMisses.Remove(eid1);
						}
					}
				}
			}
			return g;
		}
	}
}
