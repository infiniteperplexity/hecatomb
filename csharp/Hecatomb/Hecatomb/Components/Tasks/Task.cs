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
        [JsonIgnore] public int BoxWidth { get { return 1; } set { } }
        [JsonIgnore] public int BoxHeight { get { return 1; } set { } }
        [JsonProperty] private int WorkerEID;
        [JsonIgnore] public string TypeName;
        public Dictionary<string, int> Ingredients;
        protected Dictionary<int, Dictionary<string, int>> Claims;
        [JsonIgnore]
        public Creature Worker
        {
            get
            {
                if (WorkerEID == -1)
                {
                    return null;
                }
                else
                {
                    return (Creature)Game.World.Entities.Spawned[WorkerEID];
                }
            }
            protected set
            {
                if (value == null)
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
        public int Labor;

        public Task() : base()
        {
            WorkerEID = -1;
            WorkRange = 1;
            LaborCost = 10;
            Labor = LaborCost;
            Ingredients = new Dictionary<string, int>();
            Claims = new Dictionary<int, Dictionary<string, int>>();
        }

        public virtual void Standardize()
        {
            Type taskType = this.GetType();
            Task task = (Task)Activator.CreateInstance(taskType);
            WorkRange = task.WorkRange;
            LaborCost = task.LaborCost;
            MenuName = task.MenuName;
        }

        public virtual void Act()
        {
            if (Worker == null)
            {
                Debug.WriteLine("we probably shouldn't have gotten here.");
                return; // this can sometimes get unassigned in the midst of things
            }
            if (!HasIngredients() && Labor == LaborCost)
            {
                FetchIngredient();
                return;
            }
            if (Tiles.QuickDistance(Worker.X, Worker.Y, Worker.Z, Entity.X, Entity.Y, Entity.Z) <= WorkRange)
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
            Debug.WriteLine("trying to fetch an ingredient");
            if (Claims.Count == 0)
            {
                return;
            }
            int eid = Claims.Keys.ToList()[0];
            Item item = (Item) Game.World.Entities.Spawned[eid];
            // now need to do some validation
            if (!item.Placed || !item.HasResources(Claims[eid]))
            {
                Claims.Remove(eid);
            }
            // if we're standing on any claimed ingredient
            if (item.X == Worker.X && item.Y == Worker.Y && item.Z == Worker.Z)
            {
                item.RemoveResources(Claims[eid]);
                Inventory inv = Worker.GetComponent<Inventory>();
                if (inv.Item==null)
                {
                    inv.Item = Game.World.Entities.Spawn<Item>();
                }
                inv.Item.AddResources(Claims[eid]);
                item.UnclaimResources(Claims[eid]);
                Claims.Remove(eid);
                Debug.WriteLine("picking up an item");
                Worker.GetComponent<Actor>().Spend();
            }
            else
            {
                Worker.GetComponent<Actor>().WalkToward(item.X, item.Y, item.Z, useLast: true);
            }
        }

        public bool HasIngredients()
        {
            if (Ingredients.Count==0)
            {
                return true;
            }
            Inventory inv = Worker.GetComponent<Inventory>();
            // so...something weird happens here...
            if (inv.Item == null)
            {
                return false;
            }
            else
            {
                return (Worker.GetComponent<Inventory>().Item.HasResources(Ingredients));
            }
        }

        public void SpendIngredients()
        {
            if (Ingredients == null || Ingredients.Count == 0)
            {
                return;
            }
            Worker.GetComponent<Inventory>().Item.RemoveResources(Ingredients);
        }

        public virtual void Work()
        {
            if (Labor == LaborCost)
            {
                Start();
            }
            Labor -= 1;
            Worker.GetComponent<Actor>().Spend();
            if (Labor <= 0)
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
            Unassign();
            Entity.Despawn();
            Despawn();
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
            Unassign();
            Entity.Despawn();
            Despawn();
        }

        public override void Despawn()
        {
            if (Worker!=null)
            {
                Unassign();
            }
            base.Despawn();
        }

        public virtual string ListOnMenu()
        {
            return MenuName;
        }

        public virtual void SelectZone(List<Coord> squares)
        {
            foreach (Coord c in squares)
            {
                if (Game.World.Tasks[c.X, c.Y, c.Z] == null)
                {
                    TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
                    task.GetComponent<Task>().Makes = Makes;
                    task.Place(c.X, c.Y, c.Z);
                }
            }
        }

        public void Unassign()
        {
            if (Worker != null)
            {
                Worker.GetComponent<Minion>()._Unassign();
            }
            UnclaimIngredients();
        }

        public void UnclaimIngredients()
        {
            foreach (int eid in Claims.Keys)
            {
                // need some kind of null check here...or maybe a listener?
                Item item = (Item)Game.World.Entities.Spawned[eid];
                var resources = Claims[eid];
                foreach (string resource in resources.Keys)
                {
                    int n = resources[resource];
                    item.Claims[resource] -= n;
                }
            }
        }

        public virtual void SelectTile(Coord c)
        {
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null)
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
                if (Game.World.Tasks[s.X, s.Y, s.Z] == null)
                {
                    TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
                    task.GetComponent<Task>().Makes = Makes;
                    task.Place(s.X, s.Y, s.Z);
                }
            }
        }

        public virtual bool ValidTile(Coord c)
        {
            return true;
        }
           
        public virtual bool CanAssign(Creature c)
        {
            if (!Entity.Placed)
            {
                return false;
            }
            if (!ValidTile(new Coord(Entity.X, Entity.Y, Entity.Z)))
            {
                Game.StatusPanel.PushMessage("Canceling invalid task.");
                Cancel();
                return false;
            }
            Movement m = c.GetComponent<Movement>();
            bool useLast = (WorkRange == 0);
            return m.CanReach(Entity, useLast: useLast) && m.CanFindResources(Ingredients);
        }
        public virtual void AssignTo(Creature c)
        {
            c.GetComponent<Minion>()._AssignTask((TaskEntity)Entity);
            Worker = c;
            ClaimIngredients();
        }

        public void ClaimIngredients()
        {
            Dictionary<string, int> needed = new Dictionary<string, int>(Ingredients);
            List<Item> owned = Game.World.Items.Where(i => i.Owned).ToList();
            Movement m = Worker.GetComponent<Movement>();
            owned = owned.Where(it => m.CanReach(it)).ToList();
            owned = owned.OrderBy(it => { return Tiles.QuickDistance(Entity.X, Entity.Y, Entity.Z, it.X, it.Y, it.Z); }).ToList();
            // assume we have verified that there are enough
            foreach (Item item in owned)
            {
                foreach (string resource in needed.Keys.ToList())
                {
                    int claimed = (item.Claims.ContainsKey(resource)) ? item.Claims[resource] : 0;
                    int n = (item.Resources.ContainsKey(resource)) ? item.Resources[resource] : 0;
                    // if there is at least some
                    if (n - claimed > 0)
                    {                   
                        item.Claims[resource] = n;
                        if (!Claims.ContainsKey(item.EID))
                        {
                            Claims[item.EID] = new Dictionary<string, int>();
                        }
                        Claims[item.EID][resource] = n - claimed;
                        needed[resource] -= (n - claimed);
                        if (needed[resource]==0)
                        {
                            needed.Remove(resource);
                        }
                    }
                }
                if (needed.Keys.Count == 0)
                {
                    return;
                }
            }
            if (needed.Keys.Count > 0)
            {
                throw new InvalidOperationException("Apparently there weren't enough items to claim");
            }
        }
    }
}