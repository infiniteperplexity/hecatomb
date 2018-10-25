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
				Movement m = Entity.GetComponent<Movement>();
				Coord? tt = m.TryStep(t.X, t.Y, t.Z);
				if (tt==null)
				{
					WalkRandom();
				}
				else
				{
					t = (Coord) tt;
					m.StepTo(t.X, t.Y, t.Z);
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
				Coord? t = m.TryStep(x, y, z);
				if (t==null)
				{
					WalkRandom();
				}
				else
				{
					Coord c = (Coord) t;
					m.StepTo(c.X, c.Y, c.Z);
				}
			}
		}
		public void WalkRandom()
		{
			Movement m = Entity.GetComponent<Movement>();
			if (m==null) 
			{
				return;
			}
			int r = Game.World.Random.Next(4);
			Coord d = Movement.Directions4[r];
			int x1 = Entity.X + d.X;
			int y1 = Entity.Y + d.Y;
			int z1 = Entity.Z + d.Z;
			Coord? t = m.TryStep(x1, y1, z1);
			if (t==null)
			{
				Wait();
			}
			else
			{
				d = (Coord) t;
				m.StepTo(d.X, d.Y, d.Z);
			}
		}
		
		public void Wait()
		{
			Spend(ActionPoints);
		}
	}
}
