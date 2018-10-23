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
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
	public abstract class Task : Component, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
	{
		[JsonIgnore] public int BoxWidth {get {return 1;} set{}}
		[JsonIgnore] public int BoxHeight {get {return 1;} set{}}
		[JsonProperty] private int WorkerEID;
		public Dictionary<string, int> Ingredients;
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
			protected set
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
		public string Makes;
		[JsonIgnore] public int WorkRange;
		[JsonIgnore] public int LaborCost;
		[JsonIgnore] public string MenuName;
		public List<Item> Claimed;
		public int Labor;
		
		public Task() : base()
		{
			WorkerEID = -1;
			WorkRange = 1;
			LaborCost = 10;
			Labor = LaborCost;
			Claimed = new List<Item>();
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
				Debug.WriteLine("we probably shouldn't have gotten here.");
				return; // this can sometimes get unassigned in the midst of things
			}
			if (!HasIngredients() && Labor==LaborCost)
			{
				FetchIngredient();
			}
			if (Tiles.QuickDistance(Worker.X, Worker.Y, Worker.Z, Entity.X, Entity.Y, Entity.Z)<=WorkRange)
			{
				Work();
			}
			else
			{
				bool useLast = (WorkRange == 0) ? true : false;
				Worker.GetComponent<Actor>().WalkToward(Entity.X, Entity.Y, Entity.Z, useLast: useLast);
			}
		}
		
		public void FetchIngredient()
		{
			if (Claimed==null || Claimed.Count==0)
			{
				return;
			}
			Item i = Claimed[0];
			if (i.X==Worker.X && i.Y==Worker.Y && i.Z==Worker.Z)
			{
				i.Remove();
				Worker.GetComponent<Inventory>().Item = i;
				i.Claimed = false;
				Claimed.Clear();
				Worker.GetComponent<Actor>().Spend();
			}
			else
			{
				Worker.GetComponent<Actor>().WalkToward(i.X, i.Y, i.Z, useLast: true);
			}
		}
		
		public bool HasIngredients()
		{
			return (Worker.GetComponent<Inventory>().Item!=null);
		}
		
		public void SpendIngredients()
		{
			Item i = Worker.GetComponent<Inventory>().Item;
			Worker.GetComponent<Inventory>().Item = null;
			i.Despawn();
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
			SpendIngredients();
			Feature f = Game.World.Entities.Spawn<Feature>("IncompleteFeature");
			f.Place(Entity.X, Entity.Y, Entity.Z);
		}
		
		public virtual void Finish()
		{
			Complete();
		}
		
		public virtual void Complete()
		{
			Worker.GetComponent<Minion>().Unassign();
			Entity.Remove();
		}
		
		public virtual void ChooseFromMenu()
		{
			Game.Controls.Set(new SelectZoneControls(this));
		}
		
		public virtual void Designate()
		{

		}
		
		public virtual void Cancel()
		{
			Entity.Despawn();
			Despawn();
		}
		
		public virtual string ListOnMenu()
		{
			return MenuName;
		}
		
		public virtual void SelectZone(List<Coord> squares)
		{
			foreach (Coord c in squares)
			{
				if (Game.World.Tasks[c.X, c.Y, c.Z]==null) 
				{
					TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
					task.GetComponent<Task>().Makes = Makes;
					task.Place(c.X, c.Y, c.Z);
				}
			}
		}
		
		public virtual void SelectTile(Coord c)
		{
			if (Game.World.Tasks[c.X, c.Y, c.Z]==null) 
			{
				TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
				task.GetComponent<Task>().Makes = Makes;
				task.Place(c.X, c.Y, c.Z);
			}
		}
		
		public virtual void TileHover(Coord c)
		{
			
		}
		
		public virtual void TileHover(Coord c, List<Coord> squares)
		{
			
		}
		
		public virtual void BoxHover(Coord c, List<Coord> squares)
		{
			
		}
		
		public virtual void SelectBox(Coord c, List<Coord> squares)
		{
			foreach (Coord s in squares)
			{
				if (Game.World.Tasks[s.X, s.Y, s.Z]==null) 
				{
					TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
					task.GetComponent<Task>().Makes = Makes;
					task.Place(s.X, s.Y, s.Z);
				}
			}
		}
		
		public virtual bool CanAssign(Creature c)
		{
			Movement m = c.GetComponent<Movement>();
			bool useLast = (WorkRange==0);
			return m.CanReach(Entity.X, Entity.Y, Entity.Z, useLast: useLast) && CanFindIngredients();
		}
		public virtual void AssignTo(Creature c)
		{
			c.GetComponent<Minion>()._AssignTask((TaskEntity) Entity);
			Worker = c;
			ClaimIngredients();
		}
		
		public void ClaimIngredients()
		{
			if (Ingredients==null)
			{
				return;
			}
			List<Item> available = Game.World.Items.Where(i=>i.Owned && !i.Claimed).ToList();
			foreach(string s in Ingredients.Keys)
			{
				// also, must be reachable
				List<Item> items = available.Where(i=>i.TypeName==s).ToList();
				int n = 0;
				// should sort by distance
				foreach (Item i in available)
				{
					i.Claimed = true;
					Claimed.Add(i);
					n+=1;
					if (n>=Ingredients[s])
					{
					    break;
					}
				}
			}
		}
		// should this be on Worker, maybe?  I dunno...
		public bool CanFindIngredients()
		{
			if (Ingredients==null)
			{
				return true;
			}
			List<Item> available = Game.World.Items.Where(i=>i.Owned && !i.Claimed).ToList();
			foreach(string s in Ingredients.Keys)
			{
				// also, must be reachable
				List<Item> items = available.Where(i=>i.TypeName==s).ToList();
				if (items.Count<Ingredients[s])
				{
					return false;
				}
				
			}
			return true;
		}
	}
	
}