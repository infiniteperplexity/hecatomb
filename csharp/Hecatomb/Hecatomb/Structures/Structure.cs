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
            if (CountTasks()>=Width*Height)
            {
                return ge;
            }
            if (Stores.Length>0)
            {
                foreach(Item item in Items)
                {   
                    if (Stores.Contains(item.Resource) && !item.IsStored() && item.Unclaimed>0 && GetHaulTask(item.Resource)==null)
                    {
                        SpawnHaulTask(item);
                    }
                }
            }
            return ge;
        }

        public int CountTasks()
        {
            int existing = 0;
            foreach (Feature f in Features)
            {
                var (x, y, z) = f;
                if (Tasks[x, y, z] != null)
                {
                    existing++;
                }
            }
            return existing;
        }

        public HaulTask GetHaulTask(string resource)
        {
            foreach (Feature f in Features)
            {
                var (x, y, z) = f;
                Task t = Tasks[x, y, z];
                if (t is HaulTask && t.Ingredients.ContainsKey(resource))
                {
                    return t as HaulTask;
                }
            }
            return null;
        }

//        Ooh boy, this is kinda crazy.So...what's the priority here?
//1) You could make it so it only spawns one task for each resource at at time, and asks for a full stacksize.  I think this is the answer.
    // have the task constantly live until it's full...easy visual indicator
    // have the task be only one at a time, renewing
//2) You could have fixed slots for different ingredients; e.g.left side holds wood, right side holds stone.  That gets counterintuitive when you want more of one, though.
//3) You could just have it be kinda greedy and random, like I was planning on doing.That will tend to fill it all with one kind, though.
//4) You could have it be greedy but with a maximum.
        public void SpawnHaulTask(Item item)
        {
            if (CountTasks()>=Width*Height)
            {
                return;
            }
            List<int> positions = new List<int>();
            for (int i=0; i<Height*Width; i++)
            {
                positions.Add(i);
            }
            positions = positions.OrderBy(s => Game.World.Random.NextDouble()).ToList();
            // we want to do the tasks first, then the others
            foreach (int position in positions)
            {
                Feature f = Features[position];
                var (x, y, z) = f;
                Task task = Tasks[x, y, z];
                if (task!=null)
                {
                    continue;
                }
                Item current = Items[x, y, z];
                if (current==null)
                {
                    continue;


                }
                if (current.Quantity >= StackSize)

            }
                int size = item.StackSize;
                
              
                
                if (Tasks[x, y, z]==null)
                {
                    HaulTask haul = Entity.Spawn<HaulTask>();
                    haul.Ingredients = new Dictionary<string, int>() { { item.Resource, item.Unclaimed } }; ;
                    haul.Claims = new Dictionary<int, int>() { { item.EID, item.Unclaimed } };
                    haul.Place(x, y, z);
                    item.Claimed += item.Unclaimed;
                    break;
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
