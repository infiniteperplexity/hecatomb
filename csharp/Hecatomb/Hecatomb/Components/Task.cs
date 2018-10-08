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

namespace Hecatomb
{
	public abstract class Task : Component, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
	{
		public Creature Worker;
		[NonSerialized] public int WorkRange;
		[NonSerialized] public int LaborCost;
		[NonSerialized] public string MenuName;
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
			
		}
		
		public virtual void SelectTile(Coord c)
		{
			
		}
		
		public virtual void TileHover(Coord c)
		{
			
		}
	}
	
}