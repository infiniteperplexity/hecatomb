using System;
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
        [JsonIgnore] public List<string> PrereqStructures;
        [JsonIgnore] public bool ShowIngredients;
        public Dictionary<string, int> Ingredients;
        // ISelectsBox properties
        [JsonIgnore] public int BoxWidth { get { return 1; } set { } }
        [JsonIgnore] public int BoxHeight { get { return 1; } set { } }
        [JsonIgnore] public int Priority;
        // instance properties
        public TypedEntityField<Creature> Worker;
        public Dictionary<int, int> Claims;
        public string Makes;
        public int Labor;

        // constructor
        public Task() : base()
        {
            MenuName = "default task name";
            Name = "task";
            Worker = new TypedEntityField<Creature>();
            WorkRange = 1;
            LaborCost = 10;
            Labor = LaborCost;
            Priority = 5;
            Ingredients = new Dictionary<string, int>();
            Claims = new Dictionary<int,int>();
            PrereqStructures = new List<string>();
            BG = "red";
            ShowIngredients = true;
        }


        public virtual string GetHoverName()
        {
            if (Ingredients.Count == 0 || !ShowIngredients)
            {
                return Describe(article: false);
            }
            return (Describe(article: false) + " ($: " + Resource.Format(Ingredients) + ")");
        }
        // override Place from TileEntity
        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            if (x1==-1)
            {
                Debug.WriteLine("what on earth is going on here?");
                Debug.WriteLine(this);
            }
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

        // designation...by default, allow it only for explored floor tiles
        public virtual bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            if (Features[c] != null && Features[c].TryComponent<IncompleteFixtureComponent>() != null) //is this okay or does it need to be more specific?
            {
                return Features[c].GetComponent<IncompleteFixtureComponent>().Makes == Makes;
                //return false;
            }
            else if (Features[c] != null)
            {
                return false;
            }
            if (Terrains[c.X, c.Y, c.Z]==Terrain.FloorTile)
            {
                return true;
            }
            return false;
        }
        public virtual void Designate() {}

        // assignment
        public virtual bool CanAssign(Creature c)
        {
            Coord crd = new Coord(X, Y, Z);
            if (!Explored.Contains(crd) && !Options.Explored)
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
            return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanFindResources(Ingredients, alwaysNeedsIngredients: NeedsIngredients());
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
            Worker = null;
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
                if (inv.Item!=null && inv.Item.Unbox()==null)
                {
                    Debug.WriteLine("This wasn't supposed to happen");
                    Debug.WriteLine(inv.Item.EID);
                    Debug.WriteLine(inv.Item.Entity);
                }
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
            // if there are already existing claims
            foreach (int eid in Claims.Keys)
            {
                // this should always have been validated by now
                //if (Entities.ContainsKey(eid))
                Item item = (Item)Entities[eid];
                if (needed.ContainsKey(item.Resource))
                {
                    needed[item.Resource] -= Claims[eid];
                    if (needed[item.Resource] <=0 )
                    {
                        needed.Remove(item.Resource);
                    }
                }
            }
            List<Item> owned = Game.World.Items.Where(i => i.Owned).ToList();
            Movement m = Worker.GetComponent<Movement>();
            owned = owned.Where(it => m.CanReach(it)).ToList();
            owned = owned.OrderBy(it => { return Tiles.QuickDistance(X, Y, Z, it.X, it.Y, it.Z); }).ToList();
            foreach (Item item in owned)
            {
                // ideally sort this by distance first
                if (needed.ContainsKey(item.Resource))
                {
                    int needs = needed[item.Resource];
                    int claiming = Math.Min(needs, item.Unclaimed);
                    if (claiming<=0)
                    {
                        continue;
                    }
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
            // this is no longer impossible, due to reclaiming
            if (needed.Keys.Count > 0)
            {
                Debug.WriteLine("Unassigned task because ingredients can no longer be found.");
                // hard to tell whether we should wait for ingredients to arrive, or cancel the task entirely
                //Cancel();
                Unassign();
                //throw new InvalidOperationException("Apparently there weren't enough items to claim");
            }
        }

        public virtual void ValidateClaims()
        {
            int claims = Claims.Keys.Count;
            foreach (int eid in Claims.Keys.ToList())
            {
                // if it has despawned
                if (!Entities.ContainsKey(eid))
                {
                    Claims.Remove(eid);
                }
                else
                {
                    Item item = (Item)Entities[eid];
                    if (!item.Placed)
                    {
                        Claims.Remove(eid);
                    }
                }
            }
            // if any were removed, look for new ingredients
            if (Claims.Keys.Count<claims)
            {
                ClaimIngredients();
            }
        }

        public virtual Item PickUpIngredient(int eid, Item item)
        {
            return item.TakeClaimed(Claims[eid]);
        }
        public void FetchIngredient()
        {
            // make sure the claims are still valid
            ValidateClaims();
            if (Claims.Count == 0)
            {
                return;
            }
            // this will throw an error if the item has despawned
            int eid = Claims.Keys.ToList()[0];
            Item item = (Item)Entities[eid];
            // now need to do some validation
            if (item.X == Worker.X && item.Y == Worker.Y && item.Z == Worker.Z && !HasIngredient())
            {
                var (x, y, z) = item;
                Inventory inv = Worker.GetComponent<Inventory>();
                Item swap = null;
                if (inv.Item!=null)
                {
                    swap = inv.Item; 
                }
                //inv.Item = item.TakeClaimed(Claims[eid]);
                inv.Item = PickUpIngredient(eid, item);
                
                // this is a weird way to drop it...
                swap?.Place(x, y, z);
                Claims.Remove(eid);
                Worker.GetComponent<Actor>().Spend();
            }
            else
            {
                Worker.GetComponent<Actor>().WalkToward(item, useLast: true);
            }
        }
        public virtual void SpendIngredient()
        {
            if (!NeedsIngredients())
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
        public virtual void UnclaimIngredients()
        {
            foreach (int eid in Claims.Keys)
            {
                // need a null check here?
                if (Entities == null)
                {
                    // I think this happens sometimes while saving or restoring
                    return;
                }
                Item item = (Item)Entities[eid];
                item.Claimed -= Claims[eid];
            }
            Claims.Clear();
        }
        // work

        public bool CouldWorkFrom(int x, int y, int z)
        {
            return CouldWorkFrom(Worker, x, y, z);
        }
        public bool CouldWorkFrom(Creature c, int x, int y, int z)
        {
            if (WorkRange == 0 && x == X && y == Y && z == Z)
            {
                return true;
            }
            else if (WorkRange == 1 && c.GetComponent<Movement>().CouldTouch(x, y, z, X, Y, Z))
            {
                return true;
            }
            return false;
        }

        public bool CanWork()
        {
            return CanWork(Worker);
        }
        public bool CanWork(Creature c)
        {
            var (x, y, z) = c;
            if (WorkRange == 0 && x == X && y == Y && z == Z)
            {
                return true;
            }
            else if (WorkRange == 1 && c.GetComponent<Movement>().CanTouch(X, Y, Z))
            {
                return true;
            }
            return false;
        }


        public virtual void Act()
        {
            if (!Spawned)
            {
                Debug.WriteLine("Why are we trying to act with a despawned task");
            }
            if (!HasIngredient() && Labor == LaborCost)
            {
                FetchIngredient();
                return;
            }
            if (CanWork())
            {
                SpendIngredient();
                if (Ingredients.Keys.Count == 0 || (Options.NoIngredients && !NeedsIngredients()))
                {
                    Work();
                }
            }
            else
            {
                Worker.GetComponent<Actor>().WalkToward(this, useLast: (WorkRange == 0));
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
            Feature f = Game.World.Features[X, Y, Z];
            if (f != null)
            {
                if (f.TryComponent<IncompleteFixtureComponent>() != null)
                {
                    var ifc = f.GetComponent<IncompleteFixtureComponent>();
                    if (ifc.Makes == Makes)
                    {
                        return;
                    }
                    Debug.WriteLine($"But it makes {ifc.Makes} instead of {Makes}");
                }
                Debug.WriteLine("somehow trying to start building where a feature already exists"); 
            }
            f = Spawn<Feature>("IncompleteFeature");
            if (Makes != null && EntityType.Types.ContainsKey(Makes))
            {
                Feature mock = Entity.Mock<Feature>(Makes);
                f.Name = "incomplete " + mock.Name;
            }
            f.GetComponent<IncompleteFixtureComponent>().Makes = Makes;
            f.Place(X, Y, Z);
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
            if (Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
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
                if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
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
                if (!ValidTile(s))
                {
                    return;
                }
            }
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
            var c = new SelectZoneControls(this);
            c.MenuSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            ControlContext.Set(c);
        }


        protected ColoredText cachedMenuListing;
        public virtual ColoredText ListOnMenu()
        {
            if (cachedMenuListing != null)
            {
                return cachedMenuListing;
            }
            if (Ingredients.Count == 0)
            {
                cachedMenuListing = MenuName;
                return cachedMenuListing;
            }
            else if (Player.GetComponent<Movement>().CanFindResources(Ingredients))
            {
                cachedMenuListing = (MenuName + " ($: " + Resource.Format(Ingredients) + ")");
                return cachedMenuListing;
            }
            string ingredients = "{gray}" + MenuName + " ($: ";
            var keys = Ingredients.Keys.ToList();
            for (int i=0; i<keys.Count; i++)
            {
                string resource = keys[i];
                if (Player.GetComponent<Movement>().CanFindResource(resource, Ingredients[resource]))
                {
                    ingredients += ("{white}" + Resource.Format((resource, Ingredients[resource])));
                }
                else
                {
                    ingredients += ("{gray}" + Resource.Format((resource, Ingredients[resource])));
                }
                if (i < keys.Count - 1)
                {
                    ingredients += ", ";
                }
            }
            ingredients += "{gray})";
            cachedMenuListing = ingredients;
            return cachedMenuListing;
        }

        public virtual string GetHighlightColor()
        {
            return BG;
        }
    }
}
