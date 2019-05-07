using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Input;

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
        // problem!
        public List<TileEntityField<Feature>> Features;
        public Dictionary<string, int>[] Ingredients;
        public string[] Researches;
        public string[] ResearchPrereqs;
        public string[] StructurePrereqs;
        public string[] Stores;

        [JsonIgnore]
        public ResearchTask Researching
        {
            get
            {
                foreach (Feature f in Features)
                {
                    var (x, y, z) = f;
                    Task t = Game.World.Tasks[x, y, z];
                    if (t is ResearchTask)
                    {
                        return (ResearchTask)t;
                    }
                }
                return null;
            }
            set
            {

            }
        }
        public Structure()
        {
            Width = 3;
            Height = 3;
            Features = new List<TileEntityField<Feature>>();
            Ingredients = new Dictionary<string, int>[Width * Height];
            AddListener<TurnBeginEvent>(OnTurnBegin);
            ResearchPrereqs = new string[0];
            StructurePrereqs = new string[0];
            Stores = new string[0];
            Researches = new string[0];
        }

        public void InitializeFeatures()
        {
            for (int i = 0; i < Width * Height; i++)
            {
                Features.Add(null);
            }
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
                List<Item> items = Items.OrderBy((Item item) => (Tiles.QuickDistance(item.X, item.Y, item.Z, X, Y, Z))).ToList();
                foreach (string resource in Stores)
                {
                    // if there is no current haul task for that resource (fetch them one at a time)
                    if (GetHaulTask(resource)==null)
                    {
                        // loop through available items in order of distance
                        foreach (Item item in items)
                        {
                            if (item.Resource==resource && !item.IsStored() && item.Unclaimed>0)
                            {
                                TryToSpawnHaulTask(item);
                                // go to next resource
                                break;
                            }
                        }
                    }
                }
            }
            return ge;
        }
        public HaulTask GetHaulTask(string resource)
        {
            foreach (Feature f in Features)
            {
                var (x, y, z) = f;
                HaulTask ht = Tasks[x, y, z] as HaulTask;
                if (ht!=null && ht.Ingredients.ContainsKey(resource))
                {
                    return ht;
                }
            }
            return null;
        }

        public void TryToSpawnHaulTask(Item item)
        {
            string resource = item.Resource;
            List<int> order = new List<int>();
            for (int i=0; i<Width*Height; i++)
            {
                order.Add(i);
            }
            order = order.OrderBy(s => Game.World.Random.NextDouble()).ToList();
            // if there is an existing pile
            foreach (int i in order)
            {
                Feature f = Features[i];
                var (x, y, z) = f;
                Item pile = Items[x, y, z];
                Task task = Tasks[x, y, z]; //unlikely to be a haul task, should be some incidental task
                if (pile != null && pile.Resource == resource && pile.Quantity < pile.StackSize && task == null)
                {
                    HaulTask ht = Entity.Spawn<HaulTask>();
                    ht.Structure = this;
                    ht.Place(x, y, z);
                    // this logic seems correct
                    int claim = Math.Min(item.Unclaimed, pile.StackSize-pile.Quantity);
                    ht.Ingredients[resource] = claim;
                    ht.Claims[item.EID] = claim;
                    item.Claimed += claim;
                    return;
                }
            }
            // if there's no existing pile, repeat almost the same loop
            foreach (int i in order)
            {
                Feature f = Features[i];
                var (x, y, z) = f;
                Item pile = Items[x, y, z];
                Task task = Tasks[x, y, z]; //unlikely to be a haul task, should be some incidental task
                if (pile == null && task == null)
                {
                    HaulTask ht = Entity.Spawn<HaulTask>();
                    ht.Structure = this;
                    ht.Place(x, y, z);
                    // this logic seems correct
                    int claim = item.Unclaimed; ;
                    ht.Ingredients[resource] = claim;
                    ht.Claims[item.EID] = claim;
                    item.Claimed += claim;
                    return;
                }
            }
        }

        public virtual void BuildMenu(MenuChoiceControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            menu.Header = "Structure: " + Describe();
            HighlightSquares();
            if (Tasks[X, Y, Z] != null)
            {
                // ideally this should have the option to cancel research
                menu.Choices = new List<IMenuListable>();
            }
            else
            {
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
                menu.Choices = list;
            }
        }
        public virtual void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.Insert(2, "Tab) Next structure.");
            if (Researching != null)
            {
                ResearchTask rt = Researching;
                string txt = "Researching " + Research.Types[rt.Makes].Name + " (" + rt.Labor + " turns; Delete to cancel.)";
                if (rt.Ingredients.Count > 0)
                {
                    txt = "Researching " + Research.Types[rt.Makes].Name + " ($: " + Resource.Format(rt.Ingredients) + ")";
                }
                menu.MenuTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    "{yellow}Structure: "+Describe(),
                    "Tab) Next structure.",
                    txt
                };
            }
            Game.MenuPanel.Dirty = true;

            menu.KeyMap[Keys.Escape] =
                () =>
                {
                    Unhighlight();
                    Game.Controls.Reset();
                };
            menu.KeyMap[Keys.Tab] = () => { /* NextStructure */};
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
