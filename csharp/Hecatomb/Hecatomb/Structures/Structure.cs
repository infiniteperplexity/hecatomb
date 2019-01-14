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
        }

        public static List<string> ListAll()
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
            if (Researching != null)
            {
                ResearchTurns -= 1;
                if (ResearchTurns <= 0)
                {
                    var researched = Game.World.GetState<ResearchHandler>().Researched;
                    if (!researched.Contains(Researching))
                    {
                        researched.Add(Researching);
                    }
                    Researching = null;
                }
            }
            if (CountTasks()>=Width*Height)
            {
                return ge;
            }
            if (Stores.Length>0)
            {
                foreach(Item item in Items)
                {   
                    foreach (string s in Stores)
                    {
                        if (item.Resources.ContainsKey(s) && !item.IsStored(s))
                        {
                            int unclaimed = item.CountUnclaimed(s);
                            if (unclaimed > 0)
                            {
                                SpawnHaulTask(item, s, unclaimed);
                                if (CountTasks() >= Width * Height)
                                {
                                    return ge;
                                }
                            }
                        }
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
        public void SpawnHaulTask(Item item, string resource, int unclaimed)
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
            foreach (int position in positions)
            {
                Feature f = Features[position];
                var (x, y, z) = f;
                if (Tasks[x, y, z]==null)
                {
                    item.Claims[resource] += unclaimed;
                    HaulTask haul = Entity.Spawn<HaulTask>();
                    haul.Claims[item.EID] = new Dictionary<string, int>() { { resource, unclaimed } };
                    haul.Place(x, y, z);
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
                var list = new List<IMenuListable>();
                var researched = Game.World.GetState<ResearchHandler>().Researched;
                foreach (string s in Researches)
                {
                    if (!researched.Contains(s))
                    {
                        list.Add(new ResearchMenuListing(Research.Types[s], this));
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
