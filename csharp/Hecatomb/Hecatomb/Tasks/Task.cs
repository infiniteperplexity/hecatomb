﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public abstract class Task : TileEntity, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
    {
        // subclass properties
        [JsonIgnore] public int WorkRange;
        [JsonIgnore] public int LaborCost;
        [JsonIgnore] public string MenuName;
        [JsonIgnore] public Dictionary<string, int> Ingredients;
        // ISelectsBox properties
        [JsonIgnore] public int BoxWidth { get { return 1; } set { } }
        [JsonIgnore] public int BoxHeight { get { return 1; } set { } }
        // instance properties
        public TypedEntityField<Creature> Worker;
        public Dictionary<int, int> Claims;
        public string Makes;
        public int Labor;

        // constructor
        public Task() : base()
        {
            Worker = new TypedEntityField<Creature>();
            WorkRange = 1;
            LaborCost = 10;
            Labor = LaborCost;
            Ingredients = new Dictionary<string, int>();
            Claims = new Dictionary<int,int>();
        }
        // override Place from TileEntity
        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            if (Tasks[x1, y1, z1]!=null)
            {
                throw new InvalidOperationException($"Task placement conflict at {x1} {y1} {z1}");
            }
            Tasks[x1, y1, z1] = this;
            base.Place(x1, y1, z1, fireEvent);
        }
        // override Remove from TileEntity
        public override void Remove()
        {
            int x = X, y = Y, z = Z;
            base.Remove();
            Tasks[x, y, z] = null;
        }
        // override Despawn from TileEntity
        public override void Despawn()
        {
            Unassign();
            base.Despawn();
        }

        // designation
        public virtual bool ValidTile(Coord c) {return true;}
        public virtual void Designate() {}

        // assignment
        public virtual bool CanAssign(Creature c)
        {
            Coord crd = new Coord(X, Y, Z);
            if (!Explored.Contains(crd))
            {
                return false;
            }
            if (!Placed)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                Status.PushMessage("Canceling invalid task.");
                Cancel();
                return false;
            }
            Movement m = c.GetComponent<Movement>();
            return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanFindResources(Ingredients);
        }
        public virtual void AssignTo(Creature c)
        {
            c.GetComponent<Minion>().Task = this;
            Worker = c;
            ClaimIngredients();
        }
        public virtual void Cancel()
        {
            Unassign();
            Despawn();
        }
        public void Unassign()
        {
            if (Worker!=null)
            {
                Worker.GetComponent<Minion>().Task = null;
            }
            UnclaimIngredients();
        }
        public virtual bool NeedsIngredients()
        {
            return !Options.NoIngredients;
        }
        // ingredients
        public bool HasIngredient()
        {
            if (!NeedsIngredients())
            {
                return true;
            }
            if (Ingredients.Count == 0)
            {
                return true;
            }
            Inventory inv = Worker.GetComponent<Inventory>();
            if (inv.Item == null)
            {
                return false;
            }
            else
            {
                return Ingredients.ContainsKey(inv.Item.Unbox().Resource);
            }
        }
        public virtual void ClaimIngredients()
        {
            if (!NeedsIngredients())
            {
                return;
            }
            Dictionary<string, int> needed = new Dictionary<string, int>(Ingredients);
            List<Item> owned = Game.World.Items.Where(i => i.Owned).ToList();
            Movement m = Worker.GetComponent<Movement>();
            owned = owned.Where(it => m.CanReach(it)).ToList();
            owned = owned.OrderBy(it => { return Tiles.QuickDistance(X, Y, Z, it.X, it.Y, it.Z); }).ToList();
            // assume we have verified that there are enough
            foreach (Item item in owned)
            {
                // ideally sort this by distance first
                if (needed.ContainsKey(item.Resource))
                {
                    int needs = needed[item.Resource];
                    int claiming = Math.Min(needs, item.Unclaimed);
                    item.Claimed += claiming;
                    needed[item.Resource] -= claiming;
                    if (needed[item.Resource]<=0)
                    {
                        needed.Remove(item.Resource);
                    }
                    Claims[item.EID] = claiming;
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
        public void FetchIngredient()
        {
            Debug.WriteLine("trying to fetch an ingredient");
            if (Claims.Count == 0)
            {
                return;
            }
            int eid = Claims.Keys.ToList()[0];
            Item item = (Item)Entities[eid];
            // now need to do some validation
            if (!item.Placed)
            {
                Claims.Remove(eid);
                // should we look for more ingredients or just cancel the task?
            }
            // if we're standing on any claimed ingredient and 
            if (item.X == Worker.X && item.Y == Worker.Y && item.Z == Worker.Z && !HasIngredient())
            {
                var (x, y, z) = item;
                Inventory inv = Worker.GetComponent<Inventory>();
                Item swap = null;
                if (inv.Item!=null)
                {
                    swap = inv.Item; 
                }
                inv.Item = item.TakeClaimed(Claims[eid]);
                // this is a weird way to drop it...
                swap?.Place(x, y, z);
                Claims.Remove(eid);
                Worker.GetComponent<Actor>().Spend();
            }
            else
            {
                Worker.GetComponent<Actor>().WalkToward(item.X, item.Y, item.Z, useLast: true);
            }
        }
        public void SpendIngredient()
        {
            if (Options.NoIngredients)
            {
                return;
            }
            if (Ingredients == null || Ingredients.Count == 0)
            {
                return;
            }
            Item item = (Item)Worker.GetComponent<Inventory>().Item;
            Ingredients[item.Resource] -= item.Quantity;
            if (Ingredients[item.Resource]<=0)
            {
                Ingredients.Remove(item.Resource);
            }
            item.Despawn();
        }
        public void UnclaimIngredients()
        {
            foreach (int eid in Claims.Keys)
            {
                // need a null check here?
                Item item = (Item)Entities[eid];
                item.Claimed -= Claims[eid];
            }
            Claims.Clear();
        }
        // work
        public virtual void Act()
        {
            if (!HasIngredient() && Labor == LaborCost)
            {
                FetchIngredient();
                return;
            }
            if ((int)Tiles.QuickDistance(Worker.X, Worker.Y, Worker.Z, X, Y, Z) <= WorkRange)
            {
                SpendIngredient();
                if (Ingredients.Keys.Count == 0)
                {
                    Work();
                }
            }
            else
            {
                Worker.GetComponent<Actor>().WalkToward(X, Y, Z, useLast: (WorkRange == 0));
            }
        }

        public virtual void Work()
        {
            if (Labor == LaborCost)
            {
                Start();
            }
            Labor -= (1 + Options.WorkBonus);
            Worker.GetComponent<Actor>().Spend();
            if (Labor <= 0)
            {
                Finish();
            }
        }

        public virtual void Start()
        {
            Spawn<Feature>("IncompleteFeature").Place(X, Y, Z);
        }

        public virtual void Finish()
        {
            Complete();
        }

        public virtual void Complete()
        {
            Unassign();
            Despawn();
        }

        // *** interface methods
        // ISelectsTile
        public virtual void SelectTile(Coord c)
        {
            if (Tasks[c.X, c.Y, c.Z] == null)
            {
                Task task = Spawn<Task>(this.GetType());
                task.Makes = Makes;
                task.Place(c.X, c.Y, c.Z);
            }
        }
        public virtual void TileHover(Coord c) {}
        // ISelectsZone
        public virtual void SelectZone(List<Coord> squares)
        {
            foreach (Coord c in squares)
            {
                if (Game.World.Tasks[c.X, c.Y, c.Z] == null)
                {
                    Task task = Spawn<Task>(this.GetType());
                    task.Makes = Makes;
                    task.Place(c.X, c.Y, c.Z);
                }
            }
        }
        public virtual void TileHover(Coord c, List<Coord> squares) {}
        // ISelectsBox
        public virtual void SelectBox(Coord c, List<Coord> squares)
        {
            foreach (Coord s in squares)
            {
                if (Tasks[s.X, s.Y, s.Z] == null)
                {
                    Task task = (Task)Spawn(this.GetType());
                    task.Makes = Makes;
                    task.Place(s.X, s.Y, s.Z);
                }
            }
        }
        public virtual void BoxHover(Coord c, List<Coord> squares) {}
        // IMenuListable
        public virtual void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectZoneControls(this));
        }
        public virtual ColoredText ListOnMenu()
        {
            if (Ingredients.Count == 0)
            {
                return MenuName;
            }
            else if (Player.GetComponent<Movement>().CanFindResources(Ingredients))
            {
                return (MenuName + " ($: " + Resource.Format(Ingredients) + ")");
            }
            else
            {
                return ("{gray}" + MenuName + " ($: " + Resource.Format(Ingredients) + ")");
            }

        }
    }
}
