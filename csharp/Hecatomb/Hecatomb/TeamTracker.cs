/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/26/2018
 * Time: 2:11 PM
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of TeamTracker.
	/// </summary>
	/// 
	public class Team : FlyWeight
	{
		// enemies of all other teams
		bool Xenophobic;
		// enemies of all including own team
		bool Berserk;
		// list of names of enemy teams
		List<string> Enemies;
		public Team(
			string name,
			string[] enemies=null,
			bool xenophobic=false,
			bool berserk=false			
		) : base()
		{
			Name = name;
			Enemies = Enemies.ToList();
			Xenophobic = xenophobic;
			Berserk = berserk;
		}
		
//		public bool IsHostile(Creature c)
//		{
//			Actor a = c.GetComponent<Actor>();
//			return IsHostile(a);
//		}
//		public bool IsHostile(Actor a)
//		{
//			Team t = a.Team;
//			return IsHostile(t);
//		}
//		
//		public bool IsHostile(string t)
//		{
//			return IsHostile(;
//		}
		public bool IsHostile(Team t)
		{
			if (Enemies.Contains(t.Name) || t.Enemies.Contains(Name))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public List<Creature> GetEnemies()
		{
			TeamTracker tt = Game.World.GetTracker<TeamTracker>();
			List<Creature> enemies = new List<Creature>();
			foreach (string enemy in Enemies)
			{
				foreach (int eid in tt.Membership[enemy])
				{
					enemies.Add((Creature) Game.World.Entities.Spawned[eid]);
				}
			}
			return enemies;
		}
		public void AddMember(Creature c)
		{
			TeamTracker tt = Game.World.GetTracker<TeamTracker>();
			tt.Membership[Name].Add(c.EID);
		}
//		
//		public List<int> GetMembers()
//		{
//			
//		}
	}
		
			
	public class TeamTracker : StateTracker
	{
		public Dictionary<string, List<int>> Membership;
		private Dictionary<int, Dictionary<int, bool>> hostilityMatrix;
		
		public TeamTracker() : base()
		{
			Membership = new Dictionary<string, List<int>>();
			foreach(FlyWeight fl in FlyWeight.FlyWeightTypes[typeof(Team)])
			{
				Membership[fl.Name] = new List<int>();
			}
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
		
//		public void CalculateHostility()
//		{
//			foreach(
//		}
	}
}
