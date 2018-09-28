/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/25/2018
 * Time: 10:51 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	public abstract class Task : Component
	{
		public Creature Worker;
		public int Labor;
		public Task() : base()
		{
			
		}
		
		public virtual void Act()
		{
			if (Worker==null)	
			{
				return; // this can sometimes get unassigned in the midst of things
			}
			if (Tiles.QuickDistance(Worker.x, Worker.y, Worker.z, Entity.x, Entity.y, Entity.z)<=1)
			{
				Work();
			}
			else
			{
				Worker.GetComponent<Actor>().WalkToward(Entity.x, Entity.y, Entity.z);
			}
		}
		
		public virtual void Work()
		{
			Labor-=1;
			if (Labor<=0)
			{
				Finish();
			}
		}
		
		public virtual void Finish()
		{
			
		}
		
		public virtual void Complete()
		{
			Worker.GetComponent<Minion>().Task = null;
			Entity.Remove();
		}
	}
	
	public class DigTask : Task
	{
		public DigTask() : base()
		{
			Labor = 10;
		}
		
		public override void Finish()
		{
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
			var tiles = Game.World.Tiles;
			Terrain t = tiles[x, y, z];
			if (t==Terrain.WallTile)
			{
				tiles[x, y, z] = Terrain.FloorTile;
			}
			Complete();
		}
	}
}