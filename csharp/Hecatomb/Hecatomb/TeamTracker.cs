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
		) : base(name)
		{
			Name = name;
			Enemies = (enemies==null) ? new List<string>() : enemies.ToList();
			Xenophobic = xenophobic;
			Berserk = berserk;
		}
		
		public bool IsFriendly(Creature c)
		{
			Actor a = c.GetComponent<Actor>();
			return IsFriendly(a);
		}
		public bool IsFriendly(Actor a)
		{
			Team t = a.Team;
			return IsFriendly(t);
		}

		public bool IsFriendly(Team t)
		{
			if (Berserk)
			{
				return false;
			}
			if (t==this)
			{
				return true;
			}
			return false;
		}
		
		public bool IsHostile(Creature c)
		{
			Actor a = c.GetComponent<Actor>();
			return IsHostile(a);
		}
		public bool IsHostile(Actor a)
		{
			Team t = a.Team;
			return IsHostile(t);
		}

		public bool IsHostile(Team t)
		{
			if (Berserk)
			{
				return true;
			}
			if (Xenophobic && t!=this)
			{
				return true;
			}
			if (Enemies.Contains(t.Name) || t.Enemies.Contains(Name))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		// I'm not sure whether cacheing is needed here or not.
		public List<Creature> GetEnemies()
		{
			TeamTracker tt = Game.World.GetTracker<TeamTracker>();
			List<Creature> enemies = new List<Creature>();
			// this is crap...it's not symmetrical and it ignores berserk
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
		
		public void RemoveMember(Creature c)
		{
			TeamTracker tt = Game.World.GetTracker<TeamTracker>();
			tt.Membership[Name].Remove(c.EID);
		}
		
		public static Team PlayerTeam = new Team(name: "PlayerTeam");
		public static Team NeutralAnimals = new Team(name: "NeutralAnimals");
		public static Team Berserkers = new Team(name: "Berserkers", berserk: true);
//		
		
		
		
//		public List<int> GetMembers()
//		{
//			
//		}
	}
		
			
	// The teams themselves do not hold state; rather, the TeamTracker holds state for them
	// It might also do some cacheing but I'm not sure yet
	public class TeamTracker : StateTracker
	{
		public Dictionary<string, List<int>> Membership;
		private Dictionary<int, Dictionary<int, bool>> hostilityMatrix;
		
		public TeamTracker() : base()
		{
			Membership = new Dictionary<string, List<int>>();
			foreach(FlyWeight fl in FlyWeight.Enumerated[typeof(Team)])
			{
				Membership[fl.Name] = new List<int>();
			}
			
		}
		
		public override void Activate()
		{
			base.Activate();
			Game.World.Events.Subscribe<DespawnEvent>(this, OnDespawn);
		}
		
		public GameEvent OnDespawn(GameEvent ge)
		{
			Debug.WriteLine("who is getting despawned?");
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
