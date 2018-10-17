/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/25/2018
 * Time: 10:51 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
	public abstract class Task : Component, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
	{
		public int WorkerEID;
		[JsonIgnore] public Creature Worker
		{
			get
			{
				if (WorkerEID==-1)
				{
					return null;
				}
				else
				{
					return (Creature) Game.World.Entities.Spawned[WorkerEID];
				}
			}
			set
			{
				if (value==null)
				{
					WorkerEID = -1;
				}
				else
				{
					WorkerEID = value.EID;
				}
			}
		}
		[JsonIgnore] public int WorkRange;
		[JsonIgnore] public int LaborCost;
		[JsonIgnore] public string MenuName;
		public int Labor;
		
		public Task() : base()
		{
			WorkerEID = -1;
			WorkRange = 1;
			LaborCost = 10;
			Labor = LaborCost;
		}
		
		public virtual void Standardize()
		{
			Type taskType = this.GetType();
			Task task = (Task) Activator.CreateInstance(taskType);
			WorkRange = task.WorkRange;
			LaborCost = task.LaborCost;
			MenuName = task.MenuName;
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
			Worker.GetComponent<Actor>().Spend();
			if (Labor<=0)
			{
				Finish();
			}
		}
		
		public virtual void Start()
		{
			Feature f = Game.World.Entities.Spawn<Feature>("IncompleteFeature");
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
		
		public virtual void ChooseFromMenu()
		{
			Game.Controls.Set(new SelectZoneControls(this));
		}
		
		public virtual void Designate()
		{

		}
		
		public virtual string ListOnMenu()
		{
			return MenuName;
		}
		
		public virtual void SelectBox(List<Coord> squares)
		{
			
		}
		
		public virtual void SelectZone(List<Coord> squares)
		{
			foreach (Coord c in squares)
			{
				if (Game.World.Tasks[c.x, c.y, c.z]==null) 
				{
					TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
					task.Place(c.x, c.y, c.z);
				}
			}
		}
		
		public virtual void SelectTile(Coord c)
		{
			if (Game.World.Tasks[c.x, c.y, c.z]==null) 
			{
				TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
				task.Place(c.x, c.y, c.z);
			}
		}
		
		public virtual void TileHover(Coord c)
		{
			
		}
		
		public virtual void TileHover(Coord c, List<Coord> squares)
		{
			
		}
	}
	
}