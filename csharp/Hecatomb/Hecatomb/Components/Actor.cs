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
		[JsonIgnore] TypedEntity Target {
			get
			{
				return (TypedEntity) Game.World.Entities.Spawned[TargetEID];
			}
			set
			{
				TargetEID = value.EID;
			}
		}
		public Actor() : base()
		{
			
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
					Patrol(Target.x, Target.y, Target.z);
				}
				
			} else {
				Wander();
			}
			
		}
		
		public void Patrol(int x1, int y1, int z1)
		{
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
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
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
			Coord? target = Tiles.FindPath(x, y, z, x1, y1, z1, useLast: useLast);
			if (target==null)
			{
				WalkRandom();
			} else {
				Coord t = (Coord) target;
				Movement m = Entity.GetComponent<Movement>();
				if (m.CanPass(t.x, t.y, t.z))
				{
					m.StepTo(t.x, t.y, t.z);
				}
				else
				{
					WalkRandom();
				}
			}
		}
		public void WalkAway(int x1, int y1, int z1)
		{
			int x0 = Entity.x;
			int y0 = Entity.y;
			int z0 = Entity.z;
			List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
			if (line.Count<=1)
			{
				WalkRandom();
			} else
			{
				Movement m = Entity.GetComponent<Movement>();
				int x = line[0].x-x0;
				int y = line[0].y-y0;
				int z = z0;
				if (m.CanPass(x, y, z))
				{
					m.StepTo(x, y, z);
				} else {
					WalkRandom();
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
			int r = Game.Random.Next(4);
			Coord d = Movement.Directions4[r];
			int x1 = Entity.x + d.x;
			int y1 = Entity.y + d.y;
			int z1 = Entity.z + d.z;
			if (!m.CanPass(x1, y1, z1)) {
				if (m.Climbs && z1+1<Constants.DEPTH && m.CanPass(x1, y1, z1+1)){
					m.StepTo(x1, y1, z1+1);
				} else if (m.Climbs && z1-1>=0 && m.CanPass(x1, y1, z1-1)){
					m.StepTo(x1, y1, z1-1);
				}
			} else {
			    m.StepTo(x1, y1, z1);
			}
		}
	}
}
