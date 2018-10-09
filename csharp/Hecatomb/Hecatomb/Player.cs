/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/26/2018
 * Time: 10:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of Player.
	/// </summary>
	public class Player : Creature
	{
		private List<int> MinionEIDs;
		
		public bool Acted;
		public Player() : base()
		{
			Acted = false;
			MinionEIDs = new List<int>();
			
		}
		public List<Creature> GetMinions()
		{
			return MinionEIDs.Select(eid => Game.World.Entities.Spawned[eid]).Cast<Creature>().ToList();
		}
		
		public void AddMinion(Creature c)
		{
			int eid = c.EID;
			if (!MinionEIDs.Contains(eid))
			{
				MinionEIDs.Add(eid);
			}
		}
		public void HandleVisibility()
		{
			Game.Camera.Center(x, y, z);
			Game.Visible = GetComponent<Senses>().GetFOV();
			foreach(Creature c in GetMinions())
			{
				Senses s = c.GetComponent<Senses>();
				Game.Visible.UnionWith(s.GetFOV());
			}
			foreach (var t in Game.Visible)
			{
				Game.World.Explored.Add(t);
			}
		}
	}
}
