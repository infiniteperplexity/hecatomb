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
		[NonSerialized] public int WorkRange;
		[NonSerialized] public int LaborCost;
		public int Labor;
		
		public Task() : base()
		{
			WorkRange = 1;
			LaborCost = 10;
			Labor = LaborCost;
		}
		
		public virtual void Act()
		{
			if (Worker==null)	
			{
				return; // this can sometimes get unassigned in the midst of things
			}
			if (Tiles.QuickDistance(Worker.x, Worker.y, Worker.z, Entity.x, Entity.y, Entity.z)<=WorkRange)
			{
				Work();
			}
			else
			{
				bool useLast = (WorkRange == 0) ? true : false;
				Worker.GetComponent<Actor>().WalkToward(Entity.x, Entity.y, Entity.z, useLast: useLast);
			}
		}
		
		public virtual void Work()
		{
			if (Labor==LaborCost)
			{
				Start();
			}
			Labor-=1;
			if (Labor<=0)
			{
				Finish();
			}
		}
		
		public virtual void Start()
		{
			Feature f = new Feature("IncompleteFeature");
			f.Place(Entity.x, Entity.y, Entity.z);
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
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[Entity.x, Entity.y, Entity.z];
			f.Symbol = '\u2717';
			f.FG = "white";
		}
		public override void Finish()
		{
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
			Game.World.Features[x, y, z].Remove();
			var tiles = Game.World.Tiles;
			Terrain t = tiles[x, y, z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
			if (t==floor)
			{
				Terrain tb = Game.World.GetTile(x, y, z-1);
				if (tb==wall)
				{
					tiles[x, y, z] = down;
					tiles[x, y, z-1] = up;
				} else if (tb==up)
				{
					tiles[x, y, z] = down;
				}
				else if (tb==empty || tb==down || tb==floor)
				{
					tiles[x, y, z] = empty;
				}
			}
			else if (t==up)
			{
				tiles[x, y, z] = floor;
			}
			else if (t==down)
			{
				tiles[x, y, z] = empty;
				tiles[x, y, z-1] = floor;
			}
			else if (t==wall)
			{
				tiles[x, y, z] = down;
				tiles[x, y, z-1] = up;
			}		
			Complete();
		}
	}
}