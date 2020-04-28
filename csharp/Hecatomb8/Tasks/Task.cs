using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    public abstract class Task : TileEntity, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
    {
        // subclass properties
        [JsonIgnore] public bool WorkSameTile;
        public int LaborCost; // do not JsonIgnore, this can vary for some subtypes
        public int Labor;
        [JsonIgnore] public string MenuDescription;
        // probably don't want strings here...should be types, which is awkward
        [JsonIgnore] public List<Type> RequiresStructures;
        //[JsonIgnore] public bool ShowIngredients;
        public Dictionary<Resource, int> Ingredients; // do not JsonIgnore; this can vary
        // ISelectsBox properties
        [JsonIgnore] public int BoxWidth { get { return 1; } set { } }
        [JsonIgnore] public int BoxHeight { get { return 1; } set { } }
        [JsonIgnore] public int Priority;
        // instance properties
        public ListenerHandledEntityPointer<Creature>? Worker;
        public Dictionary<int, int> Claims;
        // this maybe should always be a Type?...but it should be a string for serialization?
        public Type? Makes;


        // constructor
        public Task() : base()
        {
            MenuDescription = "default task name";
            _name = "task";
            WorkSameTile = false;
            LaborCost = 10;
            Labor = LaborCost;
            Priority = 5;
            Ingredients = new Dictionary<Resource, int>();
            Claims = new Dictionary<int, int>();
            RequiresStructures = new List<Type>();
            _bg = "red";
            //ShowIngredients = true;
        }

        public GameEvent OnDespawn(GameEvent ge)
        {
            if (ge is DespawnEvent)
            {
                var de = (DespawnEvent)ge;
                if (Worker != null && de.Entity == Worker.UnboxBriefly())
                {
                    Unassign();
                }
                else if (de.Entity is Item && de.Entity.EID != null && Claims.ContainsKey((int)de.Entity.EID))
                {
                    Claims.Remove((int)de.Entity.EID);
                }
            }
            return ge;
        }

        //public virtual string GetHoverName()
        //{
        //    if (Ingredients.Count == 0 || !ShowIngredients)
        //    {
        //        return Describe(article: false);
        //    }
        //    return (Describe(article: false) + " ($: " + Resource.Format(Ingredients) + ")");
        //}
        // override Place from TileEntity
        public override void PlaceInValidEmptyTile(int x1, int y1, int z1)
        {
            if (Tasks.GetWithBoundsChecked(x1, y1, z1) != null)
            {
                throw new InvalidOperationException($"Task placement conflict at {x1} {y1} {z1}");
            }
            Tasks.SetWithBoundsChecked(x1, y1, z1, this);
            base.PlaceInValidEmptyTile(x1, y1, z1);
        }
        // override Remove from TileEntity
        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Tasks.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
        }
        // override Despawn from TileEntity
        public override void Despawn()
        {
            Unassign();
            base.Despawn();
        }

        // ValidTile is a bit of a problem child, because it pulls double duty...it checks where a new task can be assigned, and where an existing one should be canceled
        // just keep an eye out for that
        public virtual bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            // can't do most tasks where you haven't explored   
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            // can restart tasks with the same incomplete fixture
            if (f is IncompleteFixture)
            {
                return ((f as IncompleteFixture)!.Makes == this.Makes);
            }
            // can't do most tasks on a feature
            else if (f != null)
            {
                return false;
            }
            // many tasks can only be done on the floor
            if (Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z) == Terrain.FloorTile)
            {
                return true;
            }
            return false;
        }

        // assignment
        public virtual bool CanAssign(Creature c)
        {
            // just to be safe
            if (!Spawned || !Placed || !c.Spawned || !c.Placed)
            {
                return false;
            }
            Coord crd = new Coord((int)X!, (int)Y!, (int)Z!);
            // is this even worth checking?  normally you can't even assign it there
            if (!Explored.Contains(crd) && !HecatombOptions.Explored)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                PushMessage("Canceling invalid task.");
                Cancel();
                return false;
            }
            Movement m = c.GetComponent<Movement>();
            return m.CanReachBounded(this, useLast: WorkSameTile) && m.CanFindResources(Ingredients, alwaysNeedsIngredients: NeedsIngredients());
        }
        public virtual void AssignTo(Creature c)
        {
            c.GetComponent<Minion>().Task = GetPointer<Task>(c.GetComponent<Minion>().OnDespawn);
            Worker = c.GetPointer<Creature>(OnDespawn);
            ClaimIngredients();
        }
        public virtual void Cancel()
        {
            Unassign();
            Despawn();
        }
        public void Unassign()
        {
            if (Worker?.UnboxBriefly() != null)
            {
                Worker.UnboxBriefly()!.GetComponent<Minion>().Task = null;
            }
            Worker = null;
            UnclaimIngredients();
        }
        // certain tasks, like hauling and such, should require ingredients even if the debug options are set to ignore ingredients
        public virtual bool NeedsIngredients()
        {
            return !HecatombOptions.NoIngredients;
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
            if (Worker?.UnboxBriefly() is null)
            {
                return false;
            }
            Inventory inv = Worker.UnboxBriefly()!.GetComponent<Inventory>();
            if (inv.Item?.UnboxBriefly() == null)
            {
                return false;
            }
            else
            {
                return Ingredients.ContainsKey(inv.Item!.UnboxBriefly()!.Resource!);
            }
        }
        public virtual void ClaimIngredients()
        {
            if (Worker?.UnboxBriefly() is null)
            {
                Unassign();
                return;
            }
            if (!NeedsIngredients())
            {
                return;
            }
            var needed = new Dictionary<Resource, int>(Ingredients);
            // if there are already existing claims
            foreach (int eid in Claims.Keys)
            {
                // this should always have been validated by now
                //if (Entities.ContainsKey(eid))
                Item item = (Item)Entities[eid];
                if (needed.ContainsKey(item.Resource!))
                {
                    needed[item.Resource!] -= Claims[eid];
                    if (needed[item.Resource!] <= 0)
                    {
                        needed.Remove(item.Resource!);
                    }
                }
            }
            List<Item> owned = Items.Where(i => !i.Disowned).ToList();

            Movement m = Worker.UnboxBriefly()!.GetComponent<Movement>();
            owned = owned.Where(it => m.CanReachBounded(it)).ToList();
            var (X, Y, Z) = Worker.UnboxBriefly()!.GetVerifiedCoord();
            owned = owned.OrderBy(it => { return Tiles.Distance(X, Y, Z, (int)it.X!, (int)it.Y!, (int)it.Z!); }).ToList();
            foreach (Item item in owned)
            {
                // ideally sort this by distance first
                if (needed.ContainsKey(item.Resource!))
                {
                    int needs = needed[item.Resource!];
                    int claiming = Math.Min(needs, item.Unclaimed);
                    if (claiming <= 0)
                    {
                        continue;
                    }
                    item.Claimed += claiming;
                    needed[item.Resource!] -= claiming;
                    if (needed[item.Resource!] <= 0)
                    {
                        needed.Remove(item.Resource!);
                    }
                    //Owned should never return items without EIDs
                    Claims[(int)item.EID!] = claiming;
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
            if (Claims.Keys.Count < claims)
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
            if (Worker?.UnboxBriefly() is null || !Worker.UnboxBriefly()!.Placed)
            {
                Unassign();
                return;
            }
            // make sure the claims are still valid
            ValidateClaims();
            if (Claims.Count == 0)
            {
                return;
            }
            int eid = Claims.Keys.ToList()[0];
            if (!Entity.Exists(eid))
            {
                Claims.Remove(eid);
                return;
            }
            Item item = (Item)Entity.GetEntity<Item>(eid)!;
            if (!item.Placed)
            {
                Claims.Remove(eid);
                return;
            }
            var (X, Y, Z) = Worker.UnboxBriefly()!.GetVerifiedCoord();
            if (item.X == X && item.Y == Y && item.Z == Z && !HasIngredient())
            {
                var (x, y, z) = item;
                Inventory inv = Worker.UnboxBriefly()!.GetComponent<Inventory>();
                Item? swap = null;
                if (inv.Item?.UnboxBriefly() != null)
                {
                    swap = inv.Item.UnboxBriefly();
                }
                //inv.Item = item.TakeClaimed(Claims[eid]);
                var it = PickUpIngredient(eid, item);
                inv.Item = it.GetPointer<Item>(OnDespawn);
                // this is a weird way to drop it...
                swap?.DropOnValidTile(X, Y, Z);
                Claims.Remove(eid);
                Worker.UnboxBriefly()!.GetComponent<Actor>().Spend();
            }
            else
            {
                Worker.UnboxBriefly()!.GetComponent<Actor>().WalkToward(item, useLast: true);
            }
        }


        public virtual void SpendIngredient()
        {
            if (Worker?.UnboxBriefly() is null || !Worker.UnboxBriefly()!.Placed)
            {
                Unassign();
                return;
            }
            if (Worker.UnboxBriefly()!.GetComponent<Inventory>().Item?.UnboxBriefly() is null)
            {
                Unassign();
                return;
            }
            if (!NeedsIngredients())
            {
                return;
            }
            if (Ingredients == null || Ingredients.Count == 0)
            {
                return;
            }
            Item item = Worker.UnboxBriefly()!.GetComponent<Inventory>().Item!.UnboxBriefly()!;
            Ingredients[item.Resource!] -= item.N;
            if (Ingredients[item.Resource!] <= 0)
            {
                Ingredients.Remove(item.Resource!);
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
                if (Entities.ContainsKey(eid))
                {
                    Item item = (Item)Entities[eid];
                    item.Claimed -= Claims[eid];
                }
            }
            Claims.Clear();
        }
        // work

        public bool CouldWorkFrom(int x, int y, int z)
        {
            if (Worker?.UnboxBriefly() is null)
            {
                return false;
            }
            return CouldWorkFrom(Worker.UnboxBriefly()!, x, y, z);
        }
        public bool CouldWorkFrom(Creature c, int x, int y, int z)
        {
            if (!Placed || !c.Placed)
            {
                return false;
            }
            var (X, Y, Z) = GetVerifiedCoord();
            if (WorkSameTile && x == X && y == Y && z == Z)
            {
                return true;
            }
            else if (!WorkSameTile && c.GetComponent<Movement>().CouldTouchBounded(x, y, z, X, Y, Z))
            {
                return true;
            }
            return false;
        }

        public bool CanWork()
        {
            if (Worker?.UnboxBriefly() is null)
            {
                return false;
            }    
            return CanWork(Worker.UnboxBriefly()!);
        }
        public bool CanWork(Creature c)
        {
            if (!Placed)
            {
                return false;
            }
            var (x, y, z) = c;
            var (X, Y, Z) = GetVerifiedCoord();
            if (WorkSameTile && x == X && y == Y && z == Z)
            {
                return true;
            }
            else if (!WorkSameTile && c.GetComponent<Movement>().CanTouchBounded(X, Y, Z))
            {
                return true;
            }
            return false;
        }


        public virtual void Act()
        {
            if (!Spawned || !Placed || Worker?.UnboxBriefly() is null || !Worker.UnboxBriefly()!.Placed)
            {
                Unassign();
                return;
            }
            // changed this to >= to protect against certain bugs that are probably fixed already
            if (!HasIngredient() && Labor >= LaborCost)
            {
                FetchIngredient();
                return;
            }
            if (CanWork())
            {
                SpendIngredient();
                if (Ingredients.Keys.Count == 0 || (HecatombOptions.NoIngredients && !NeedsIngredients()))
                {
                    Work();
                }
            }
            else
            {
                Worker.UnboxBriefly()!.GetComponent<Actor>().WalkToward(this, useLast: (WorkSameTile));
            }
        }

        public virtual void Work()
        {
            if (!Spawned || !Placed || Worker?.UnboxBriefly() is null || !Worker.UnboxBriefly()!.Placed)
            {
                Unassign();
                return;
            }
            var (X, Y, Z) = GetVerifiedCoord();
            if (!ValidTile(new Coord(X, Y, Z)))
            {
                Cancel();
                return;
            }
            // should we check if it's a validTile each time we work?
            if (Labor >= LaborCost)
            {
                Start();
            }
            if (!ValidTile(new Coord(X, Y, Z)))
            {
                Cancel();
                return;
            }
            Labor -= (1 + HecatombOptions.WorkBonus);
            Worker.UnboxBriefly()!.GetComponent<Actor>().Spend();
            if (!ValidTile(new Coord(X, Y, Z)))
            {
                Cancel();
                return;
            }
            if (Labor <= 0)
            {
                Finish();
            }
        }

        public virtual void Start()
        {
            if (!Spawned || !Placed)
            {
                return;
            }
            var (X, Y, Z) = GetVerifiedCoord();
            Feature? f = Features.GetWithBoundsChecked(X, Y, Z);
            if (f != null)
            {
                if (f is IncompleteFixture)
                {
                    var ifc = (IncompleteFixture)f;
                    if (ifc.Makes == Makes)
                    {
                        return;
                    }
                }
                Cancel();
                return;
            }
            var ifx = Spawn<IncompleteFixture>();
            ifx.Makes = Makes;
            ifx.PlaceInValidEmptyTile(X, Y, Z);
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
        // ISelectsTile (I don't think we need to remind myself to check bounds, since this is only called from a few places)
        public virtual void SelectTile(Coord c)
        {
            if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null)
            //if (Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
            {
                Task task = Spawn<Task>(this.GetType());
                task.Makes = Makes;
                task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
            }
        }
        public virtual void TileHover(Coord c) { }
        // ISelectsZone
        public virtual void SelectZone(List<Coord> squares)
        {
            foreach (Coord c in squares)
            {
                if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) == null)
                //if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
                {
                    Task task = Spawn<Task>(this.GetType());
                    task.Makes = Makes;
                    task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                }
            }
        }
        public virtual void TileHover(Coord c, List<Coord> squares) { }
        // ISelectsBox
        public virtual void SelectBox(List<Coord> squares)
        {
            foreach (Coord s in squares)
            {
                //if (!ValidTile(s))
                //{
                //    return;
                //}
            }
            foreach (Coord s in squares)
            {
                if (Tasks.GetWithBoundsChecked(s.X, s.Y, s.Z) == null)
                {
                    Task task = (Task)Spawn(this.GetType());
                    task.Makes = Makes;
                    task.PlaceInValidEmptyTile(s.X, s.Y, s.Z);
                }
            }
        }
        public virtual void BoxHover(Coord c, List<Coord> squares) { }
        // IMenuListable
        public virtual void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectZoneControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            InterfaceState.SetControls(c);
        }


        //protected ColoredText cachedMenuListing;
        public virtual ColoredText ListOnMenu()
        {
            return MenuDescription;
            //if (cachedMenuListing != null)
            //{
            //    return cachedMenuListing;
            //}
            //if (Ingredients.Count == 0)
            //{
            //    cachedMenuListing = MenuName;
            //    return cachedMenuListing;
            //}
            //else if (Player.GetComponent<Movement>().CanFindResources(Ingredients, useCache: false))
            //{
            //    cachedMenuListing = (MenuName + " ($: " + Resource.Format(Ingredients) + ")");
            //    return cachedMenuListing;
            //}
            //string ingredients = "{gray}" + MenuName + " ($: ";
            //var keys = Ingredients.Keys.ToList();
            //for (int i = 0; i < keys.Count; i++)
            //{
            //    string resource = keys[i];
            //    if (Player.GetComponent<Movement>().CanFindResource(resource, Ingredients[resource], useCache: false))
            //    {
            //        ingredients += ("{white}" + Resource.Format((resource, Ingredients[resource])));
            //    }
            //    else
            //    {
            //        ingredients += ("{gray}" + Resource.Format((resource, Ingredients[resource])));
            //    }
            //    if (i < keys.Count - 1)
            //    {
            //        ingredients += ", ";
            //    }
            //}
            //ingredients += "{gray})";
            //cachedMenuListing = ingredients;
            //return cachedMenuListing;
        }

        public virtual string GetHighlightColor()
        {
            return BG!;
        }
    }
}
