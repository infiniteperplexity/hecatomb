/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:55 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of MinionsOwner.
	/// </summary>
	public class Minions : Component
	{
		[JsonIgnore] public int Count
		{
			get
			{
				return MinionEIDs.Count;
			}
			set {}
		}
		[JsonProperty] private List<int> MinionEIDs;
		public Minions() : base()
		{
			MinionEIDs = new List<int>();
		}
		
		public Creature this[int i]
	   	{
			get
			{	
				return (Creature) Entities[MinionEIDs[i]];
			}
			set
			{
				MinionEIDs[i] = value.EID;
			}
		}
		
		public void Add(Creature m)
		{
			MinionEIDs.Add(m.EID);
			m.GetComponent<Actor>().Team = Team.PlayerTeam;
		}
		public void Remove(Creature m)
		{
			MinionEIDs.Remove(m.EID);
		}
		public bool Contains(Creature m)
		{
			return MinionEIDs.Contains(m.EID);
		}

        public List<Creature> GetList()
        {
            return MinionEIDs.Select(eid => (Creature) Entities[eid]).ToList();
        }
	}
}
