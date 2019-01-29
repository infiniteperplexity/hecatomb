using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    using static HecatombAliases;

    public abstract class Structure : TileEntity, IChoiceMenu
    {
        public string MenuName;
        public int Width;
        public int Height;
        public char[] Symbols;
        public string[] FGs;
        public string[] BGs;
        public Feature[] Features;
        public Dictionary<string, int>[] Ingredients;
        public string[] Researches;
        public string[] ResearchPrereqs;
        public string[] StructurePrereqs;
        public string Researching;
        public int ResearchTurns;
        public string[] Stores;

        public Structure()
        {
            Width = 3;
            Height = 3;
            Features = new Feature[Width * Height];
            Ingredients = new Dictionary<string, int>[Width * Height];
            AddListener<TurnBeginEvent>(OnTurnBegin);
            ResearchPrereqs = new string[0];
            StructurePrereqs = new string[0];
            Stores = new string[0];
            Researches = new string[0];
        }

        public static List<string> ListAsStrings()
        {
            List<Entity> list = Entities.Values.Where(((Entity e) =>
            {
                if (!(e is Structure))
                {
                    return false;
                }
                else
                {
                    var se = (Structure)e;
                    if (se.Placed)
                    {
                        return true;
                    }
                    return false;
                }
            })).ToList();
            List<string> s = list.Select(e => e.ClassName).ToList();
            return s;
        }

        public static List<Structure> ListStructures()
        {
            List<Entity> list = Entities.Values.Where(((Entity e) =>
            {
                if (!(e is Structure))
                {
                    return false;
                }
                else
                {
                    var se = (Structure)e;
                    if (se.Placed)
                    {
                        return true;
                    }
                    return false;
                }
            })).ToList();
            List<Structure> s = list.Select(e => (Structure) e).ToList();
            return s;
        }

        public Dictionary<string, int> GetIngredients()
        {
            var ingredients = new Dictionary<string, int>();
            foreach (var resources in Ingredients)
            {
                if (resources != null)
                {
                    foreach (string resource in resources.Keys)
                    {
                        if (!ingredients.ContainsKey(resource))
                        {
                            ingredients[resource] = 0;
                        }
                        ingredients[resource] += resources[resource];
                    }
                }
            }
            return ingredients;
        }

        public virtual GameEvent OnTurnBegin(GameEvent ge)
        {
            if (!Placed)
            {
                return ge;
            }
            if (Stores.Length > 0)
            {
                foreach (Item item in Items)
                {
                    if (Stores.Contains(item.Resource) && !item.IsStored() && item.Unclaimed > 0)
                    {
                        AssignItemToHaulTask(item);
                    }
                }
            }
            return ge;
        }

        public void AssignItemToHaulTask(Item item)
        {
            string resource = item.Resource;
            List<int> order = new List<int>();
            for (int i=0; i<Width*Height; i++)
            {
                order.Add(i);
            }
            order = order.OrderBy(s => Game.World.Random.NextDouble()).ToList();
            // if there's an existing task with space left
            foreach (int i in order)
            {
                Feature f = Features[i];
                var (x, y, z) = f;
                Task task = Tasks[x, y, z];
                Item pile = Items[x, y, z];
                if (task != null && task is HaulTask)
                {
                    HaulTask ht = (HaulTask)task;
                    int space = ht.HasSpace(resource);
                    string res = ht.Ingredients.Keys.ToList()[0];
                    if (res == resource && space > 0)
                    {
                        int claim = Math.Min(item.Unclaimed, space);
                        if (!ht.Claims.ContainsKey(item.EID))
                        {
                            ht.Claims[item.EID] = 0;
                        }
                        ht.Claims[item.EID] += claim;
                        item.Claimed += claim;
                        return;
                    }
                }
            }
            // if there isn't an existing task with space left
            foreach (int i in order)
            {
                Feature f = Features[i];
                var (x, y, z) = f;
                Task task = Tasks[x, y, z];
                Item pile = Items[x, y, z];
                if (task == null)
                {
                    HaulTask ht = Entity.Spawn<HaulTask>();
                    ht.Place(x, y, z);
                    ht.Ingredients[resource] = 0;
                    int claim = Math.Min(item.Unclaimed, ht.HasSpace(resource));
                    ht.Ingredients[resource] = claim;
                    ht.Claims[item.EID] = claim;
                    item.Claimed += claim;
                    return;
                }
            }
        }
       

        public virtual string MenuHeader
        {
            get
            {
                return String.Format("{3} at {0} {1} {2}", X, Y, Z, Name);
            }
            set { }
        }

        public virtual List<IMenuListable> MenuChoices
        {
            get
            {
                if (Tasks[X, Y, Z]!=null)
                {
                    // ideally this should have the option to cancel research
                    return new List<IMenuListable>();
                }
                var list = new List<IMenuListable>();
                var researched = Game.World.GetState<ResearchHandler>().Researched;
                foreach (string s in Researches)
                {
                    if (!researched.Contains(s))
                    {
                        Research research = Hecatomb.Research.Types[s];
                        ResearchTask rt = Entity.Mock<ResearchTask>();
                        rt.Makes = research.TypeName;
                        rt.LaborCost = research.Turns;
                        rt.Ingredients = research.Ingredients;
                        rt.Structure = this;
                        list.Add(rt);
                    }
                }
                return list;
            }
            set { }
        }

        public void HighlightSquares(string s)
        {
            foreach (Feature fr in Features)
            {
                fr.Highlight = s;
            }
        }

        public void HighlightSquares()
        {
            HighlightSquares("lime green");
        }
        public void Unhighlight()
        {
            foreach (Feature fr in Features)
            {
                fr.Highlight = null;
            }
        }

    }
}
