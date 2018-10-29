/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 1:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Actor.
	/// </summary>
	/// 
	
	public class Actor : Component
	{
		public int TargetEID;
		public int ActionPoints;
		public int CurrentPoints;
		private string TeamName;
		[JsonIgnore] public Team Team
		{
			get
			{
				return (TeamName==null) ? null : (Team) FlyWeight.Types[typeof(Team)][TeamName];
			}
			set
			{
				if (value==null)
				{
					if (TeamName!=null)
					{
						Team t = (Team) FlyWeight.Types[typeof(Team)][TeamName];
						t.RemoveMember((Creature) Entity);
					}
					TeamName = null;
				}
				else
				{
					value.AddMember((Creature) Entity);
					TeamName = value.Name;
				}
			}
		}
		
		[JsonIgnore] TypedEntity Target {
			get
			{
				if (TargetEID==-1)
				{
					return null;
				}
				else 
				{
					return (TypedEntity) Game.World.Entities.Spawned[TargetEID];
				}
			}
			set
			{
				if (value==null)
				{
					TargetEID = -1;
				}
				else
				{
					TargetEID = value.EID;
				}
			}
		}
		public Actor() : base()
		{
			TargetEID = -1;
			ActionPoints = 16;
			CurrentPoints = (Game.World.Turns.Turn==0) ? ActionPoints : 0;
		}
		
		public void Regain()
		{
			while (CurrentPoints<=0)
			{
				CurrentPoints+=16;
			}
		}
		public void Spend(int i)
		{
			CurrentPoints-=i;
		}
		public void Spend()
		{
			CurrentPoints-=16;
		}
		public void Act()
		{
			if (Entity is Player)
			{
				return;
			}
			// in the JS version, there are several different modules on here...
			// are they flyweights?  They might be
			Minion m = Entity.TryComponent<Minion>();
			if (m!=null)
			{				
				if (m.Task!=null)
				{
					Target = m.Task;
					m.Task.GetComponent<Task>().Act();
				} else {
					Target = Game.World.Player;
					Patrol(Target.X, Target.Y, Target.Z);
				}
				
			} else {
				Wander();
			}
			
		}
		
		public void Patrol(int x1, int y1, int z1)
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			int d = (int) Tiles.QuickDistance(x, y, z, x1, y1, z1);
			if (d>=5)
			{	
				WalkToward(x1, y1, z1);
			} else if (d<=2) 
			{
				WalkAway(x1, y1, z1);
			} else {
				WalkRandom();
			}
		}
		
		public void Wander() {
			WalkRandom();
		}
//		public override void OnAddToEntity() {
//			base.OnAddToEntity();
//			// nothing for now
//		}
		public void WalkToward(int x1, int y1, int z1, bool useLast=false)
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			var path = Tiles.FindPath(x, y, z, x1, y1, z1, useLast: useLast);
			Coord? target = (path.Count>0) ? path.First.Value : (Coord?) null;
			//Coord? target = Tiles.FindPath(x, y, z, x1, y1, z1, useLast: useLast);
			if (target==null)
			{
				WalkRandom();
			} else {
				Coord t = (Coord) target;
				bool acted = TryStepTo(t.X, t.Y, t.Z);
				if (!acted)
				{
					WalkRandom();
				}
			}
		}
		public void WalkAway(int x1, int y1, int z1)
		{
			int x0 = Entity.X;
			int y0 = Entity.Y;
			int z0 = Entity.Z;
			List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
			if (line.Count<=1)
			{
				WalkRandom();
			} else
			{
				Movement m = Entity.GetComponent<Movement>();
				int x = line[0].X-x0;
				int y = line[0].Y-y0;
				int z = z0;
				bool acted = TryStepTo(x, y, z);
				if (!acted)
				{
					WalkRandom();
				}
			}
		}
		public void WalkRandom()
		{
			int r = Game.World.Random.Next(4);
			Coord d = Movement.Directions4[r];
			int x1 = Entity.X + d.X;
			int y1 = Entity.Y + d.Y;
			int z1 = Entity.Z + d.Z;
			bool acted = TryStepTo(x1, y1, z1);
			if (!acted)
			{
				Wait();
			}
		}
		
		
		public bool TryStepTo(int x1, int y1, int z1)
		{
			Debug.WriteLine(Team);
			Movement m = Entity.GetComponent<Movement>();
			// okay what's up here?
			int x = x1-Entity.X;
			int y = y1-Entity.Y;
			int z = z1-Entity.Z;
			Coord c = new Coord(x, y, z);
			Creature cr;
			// we want to loop through the base
			Coord[][] fallbacks;
			// use fallbacks for ordinary, directional movement
			if (Movement.Fallbacks.ContainsKey(c))
			{
				fallbacks = Movement.Fallbacks[c];
			}
			else
			{
				// otherwise mock up an array with only one row
				fallbacks = new Coord[][] {new Coord[] {c}};
			}
			foreach (var row in fallbacks)
			{
				foreach (Coord dir in row)
				{
					x = dir.X+Entity.X;
					y = dir.Y+Entity.Y;
					z = dir.Z+Entity.Z;
					cr = Game.World.Creatures[x, y, z];
					if (m.CanPass(x, y, z))
					{
						m.StepTo(x, y, z);
						return true;
					}
					else if (cr!=null && m.CanMove(x, y, z) && Team.IsFriendly(cr) && cr!=Game.World.Player)
					{
						Minion minion = cr.TryComponent<Minion>();
						if (minion!=null && minion.Task!=null)
						{
							// for now, make working creatures unpushable
							// eventually, try to push creature into a neighboring square where it can still work
							
							continue;
						}
						if (Game.World.Random.NextDouble()<0.5)
						{
							m.Displace(cr);
							return true;
						}
					}
				}
			}
			return false;
		}
		public void Wait()
		{
			Spend(ActionPoints);
		}
		
		public void CheckForHostile()
		{
			// so...back in the JS version, this totally flogged performance.
			// we could rebuild the hostility matrix every time a team changes...
		}
	}
}
