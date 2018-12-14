/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/26/2018
 * Time: 2:11 PM
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
	public class TeamTracker : StateHandler
	{
		public Dictionary<string, List<int>> Membership;
		private Dictionary<int, Dictionary<int, bool>> hostilityMatrix;
		
		public TeamTracker() : base()
		{
			Membership = new Dictionary<string, List<int>>();
            foreach (Team t in Team.Enumerated)
            {
                Membership[t.TypeName] = new List<int>();
            }
		}
		
		public override void Activate()
		{
			base.Activate();
			Game.World.Events.Subscribe<DespawnEvent>(this, OnDespawn);
		}
		
		public GameEvent OnDespawn(GameEvent ge)
		{
			DespawnEvent ds = (DespawnEvent) ge;
			foreach (List<int> members in Membership.Values)
			{
				foreach (int eid in members.ToList())
				{
					if (ds.Entity.EID==eid)
					{
						members.Remove(eid);
					}
				}
			}
			return ge;
		}
	}
}
