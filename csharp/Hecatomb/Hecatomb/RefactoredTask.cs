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

    class Refactor : TileEntity, IMenuListable, ISelectsBox, ISelectsTile, ISelectsZone
    {
        // subclass properties
        [JsonIgnore] public int WorkRange;
        [JsonIgnore] public int LaborCost;
        [JsonIgnore] public string MenuName;
        [JsonIgnore] public int BoxWidth;
        [JsonIgnore] public int BoxHeight;
        [JsonIgnore] public Dictionary<string, int> Ingredients;

        // instance properties
        public EntityField<Creature> Worker;
        protected Dictionary<int, Dictionary<string, int>> Claims;
        public string Makes;
        public int Labor;

        // constructor
        public Refactor() : base()
        {
            WorkRange = 1;
            LaborCost = 10;
            Labor = LaborCost;
            Ingredients = new Dictionary<string, int>();
            Claims = new Dictionary<int, Dictionary<string, int>>();
        }
        // override Place from TileEntity
        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Refactor existing = Tasks[x1, y1, z1];
            if (existing == null)
            {
                Tasks[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else
            {
                throw new InvalidOperationException(String.Format(
                    "Cannot place {0} at {1} {2} {3} because {4} is already there.", ClassName, x1, y1, z1, existing.ClassName
                ));
            }
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
            if (Worker==null)
            {
                Unassign();
            }
            base.Despawn();
        }

        // behavior for assigned worker
        public virtual void Act()
        {
            if (!HasIngredients() && Labor == LaborCost)
            {
                FetchIngredient();
                return;
            }
            // !!!! refactor
            if ((int)Tiles.QuickDistance(Worker.X, Worker.Y, Worker.Z, Entity.X, Entity.Y, Entity.Z) <= WorkRange)
            {
                Work();
            }
            else
            {
                bool useLast = (WorkRange == 0) ? true : false;
                Worker.GetComponent<Actor>().WalkToward(X, Entity.Y, Entity.Z, useLast: useLast);
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
            if (!item.Placed || !item.HasResources(Claims[eid]))
            {
                Claims.Remove(eid);
            }
            // if we're standing on any claimed ingredient
            if (item.X == Worker.X && item.Y == Worker.Y && item.Z == Worker.Z)
            {
                item.RemoveResources(Claims[eid]);
                Inventory inv = Worker.GetComponent<Inventory>();
                if (inv.Item == null)
                {
                    inv.Item = Hecatomb.Entity.Spawn<Item>();
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
            if (Game.Options.NoIngredients)
            {
                return true;
            }
            if (Ingredients.Count == 0)
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
            if (Game.Options.NoIngredients)
            {
                return;
            }
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
            Feature f = Hecatomb.Entity.Spawn<Feature>("IncompleteFeature");
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
    }
}
