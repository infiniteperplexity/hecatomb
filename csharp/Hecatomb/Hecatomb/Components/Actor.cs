/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 1:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Actor.
	/// </summary>
	public class Actor : Component
	{
		public Actor()
		{
			
		}
		
		public void Act()
		{
			TypedEntity p = Game.Player;
			Patrol(p.x, p.y, p.z);
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
			} else if (d<=1) 
			{
				WalkRandom();
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
		public void WalkToward(int x1, int y1, int z1)
		{
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
			Coord? target = Tiles.FindPath(x, y, z, x1, y1, z1);
			if (target==null)
			{
				WalkRandom();
			} else {
				Coord t = (Coord) target;
				Movement m = Entity.GetComponent<Movement>();
				m.StepTo(t.x, t.y, t.z);
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
