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
        public Dictionary<string, float>[] Harvests;
        public string[] Researches;
        public string[] ResearchPrereqs;
        public string[] StructurePrereqs;
        public string[] Stores;
        public List<Coord> Squares;
        public string UseHint;

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
            Harvests = new Dictionary<string, float>[Width * Height];
            AddListener<TurnBeginEvent>(OnTurnBegin);
            AddListener<DespawnEvent>(OnDespawn);
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
                            if (item.Resource==resource && !item.IsStored() && item.Owned && item.Unclaimed>0)
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
            ControlContext.Selection = this;
            // this is kind of weird...in BuildMenu we check for a task, in FinishMenu we instead check for Researching
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
                        rt.MenuName = "research " + research.Name;
                        rt.LaborCost = research.Turns;
                        rt.Ingredients = research.Ingredients;
                        rt.Structure = this;
                        list.Add(rt);
                    }
                }
                menu.Choices = list;
            }
        }

        // we are passed Cancel, , (blank header), (choices)
        public virtual void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.Insert(2, "Tab) Next structure.");
            menu.MenuTop.Insert(3, " ");
            menu.MenuTop.Insert(4, "{yellow}" + Describe(capitalized: true));
            menu.MenuTop.Insert(5, "{light cyan}" + UseHint);
            if (Researching != null)
            {
                ResearchTask rt = Researching;
                menu.MenuTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    " ",
                    "Tab) Next structure.",
                    " ",
                    "{yellow}" + Describe(capitalized: true),
                    "{light cyan}" + UseHint,
                    " ",
                    "Researching " + Research.Types[rt.Makes].Name + " (" + rt.Labor + " turns.)",
                    "(Backspace/Del to Cancel)"
                };
                if (rt.Ingredients.Count > 0 && !Options.NoIngredients)
                {
                    menu.MenuTop[7] = "Researching " + Research.Types[rt.Makes].Name + " ($: " + Resource.Format(rt.Ingredients) + ")";
                }
            }
            if (Stores.Length > 0)
            {
                var stored = GetStored();
                if (stored.Count > 0)
                {
                    menu.MenuTop.Add(" ");
                    menu.MenuTop.Add("Stored:");
                }
                foreach (string res in stored.Keys)
                {
                    var r = Resource.Types[res];
                    menu.MenuTop.Add("{" + r.ListColor + "}- " + Resource.Format((res, stored[res])));
                }
            }
            Game.InfoPanel.Dirty = true;
            //InterfacePanel.DirtifySidePanels();
            menu.KeyMap[Keys.Escape] =
                () =>
                {
                    ControlContext.Selection = null;
                    ControlContext.Reset();
                };
            menu.KeyMap[Keys.Tab] = NextStructure;
            menu.KeyMap[Keys.Delete] = CancelResearch;
            menu.KeyMap[Keys.Back] = CancelResearch;
        }

        public void CancelResearch()
        {
            if (Researching != null)
            {
                Task t = Tasks[X, Y, Z];
                t?.Cancel();
                Researching = null;
            }
            InterfacePanel.DirtifyUsualPanels();
        }

        public Dictionary<string, int> GetStored()
        {
            var stored = new Dictionary<string, int>();
            if (Stores.Length > 0)
            {
                foreach (Feature f in Features)
                {
                    var (x, y, z) = f;
                    Item item = Items[x, y, z];
                    if (item != null && Stores.Contains(item.Resource))
                    {
                        if (stored.ContainsKey(item.Resource))
                        {
                            stored[item.Resource] += item.Quantity;
                        }
                        else
                        {
                            stored[item.Resource] = item.Quantity;
                        }
                    }
                }
            }
            return stored;
        }

        public void NextStructure()
        {
            var structures = ListStructures();
            if (structures.Count>0)
            {
                int n = -1;
                for (int i=0; i<structures.Count; i++)
                {
                    if (structures[i]==this)
                    {
                        n = i;
                    }
                }
                if (n==-1 || n==structures.Count-1)
                {
                    ControlContext.Set(new MenuChoiceControls((Structure)structures[0]));
                }
                else
                {
                    ControlContext.Set(new MenuChoiceControls((Structure)structures[n+1]));
                }
            }
        }
        public void BuildInSquares(List<Coord> squares)
        {
            Squares = squares;
            BuildInSquares();
        }
        public void BuildInSquares()
        {
            for (int i = 0; i < Squares.Count; i++)
            {
                Coord s = Squares[i];
                Feature f = Game.World.Features[s.X, s.Y, s.Z];
                if (Game.World.Tasks[s.X, s.Y, s.Z] != null)
                {
                    return;
                }
                else if (f != null)
                {
                    if (f.TryComponent<IncompleteFixtureComponent>() != null && f.GetComponent<IncompleteFixtureComponent>().Structure == this)
                    {
                        // this is okay, go ahead
                    }
                    else if (f.TryComponent<StructuralComponent>() != null && f.GetComponent<StructuralComponent>().Structure == this)
                    {
                        // this is okay, go ahead
                    }
                    else
                    {
                        // this is not okay, cancel building
                        return;
                    }
                }
            }
            for (int i = 0; i < Squares.Count; i++)
            {
                Coord s = Squares[i];
                if (Game.World.Tasks[s.X, s.Y, s.Z] == null)
                {
                    Feature f = Game.World.Features[s.X, s.Y, s.Z];
                    if (f == null || f.TypeName=="IncompleteFeature")
                    {
                        ConstructTask tc = Entity.Spawn<ConstructTask>();
                        tc.Makes = ClassName;
                        tc.Structure = this;
                        tc.FeatureIndex = i;
                        tc.Ingredients = Ingredients[i] ?? new Dictionary<string, int>();
                        tc.Harvests = (Harvests.Length> i) ? Harvests[i] : new Dictionary<string, float>();
                        tc.Place(s.X, s.Y, s.Z);
                    }
                    else if (f.TryComponent<StructuralComponent>()!=null)
                    {
                        StructuralComponent sc = f.GetComponent<StructuralComponent>();
                        if (sc.Structure!=this)
                        {
                            throw (new InvalidOperationException("You're trying to build one structure on top of another!"));
                        }
                        else
                        {
                            Defender d = f.GetComponent<Defender>();
                            if (d.Wounds > 0)
                            {
                                RepairTask rt = Entity.Spawn<RepairTask>();
                                rt.Ingredients = Ingredients[i] ?? new Dictionary<string, int>();
                                rt.Place(s.X, s.Y, s.Z);
                            }
                        }
                    }
                }
            }
        }

        // check to see if all traces of this building have been wiped out
        public bool AtLeastPartiallyExists(TileEntity despawning)
        {
            if (Placed)
            {
                return true;
            }
            else
            {
                bool exists = false;
                foreach (Coord s in Squares)
                {
                    Feature f = Game.World.Features[s.X, s.Y, s.Z];
                    if (f != null && f != despawning)
                    {
                        var sc = f.TryComponent<StructuralComponent>();
                        if (sc != null)
                        {
                            if (sc.Structure == this)
                            {
                                exists = true;
                            }
                        }
                        var ifc = f.TryComponent<IncompleteFixtureComponent>();
                        if (ifc != null)
                        {
                            if (ifc.Structure == this)
                            {
                                exists = true;
                            }
                        }
                    }
                    Task t = Game.World.Tasks[s.X, s.Y, s.Z];
                    if (t!=null && t!=despawning && t is ConstructTask)
                    {
                        if (t.Makes==this.ClassName)
                        {
                            exists = true;
                        }
                    }
                }
                return exists;
            }
        }
        public GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity is Feature)
            {
                Feature f = (Feature)de.Entity;
                if (Placed && Features.Contains(f))
                {
                    Remove();
                    Features = new List<TileEntityField<Feature>>();
                    if (!AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                }
                if (!Placed)
                {
                    var ifc = f.TryComponent<IncompleteFixtureComponent>();
                    if (ifc!=null && ifc.Structure==this && !AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                    var sc = f.TryComponent<StructuralComponent>();
                    if (sc != null && sc.Structure == this && !AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                }
            }
            if (de.Entity is ConstructTask)
            {
                if (!AtLeastPartiallyExists((ConstructTask) de.Entity))
                {
                    Despawn();
                }
            }
            return ge;
        }
    }
}
