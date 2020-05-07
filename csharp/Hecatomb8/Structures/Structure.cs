using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    using static HecatombAliases;

    public abstract class Structure : TileEntity, IDisplayInfo
    {
        [JsonIgnore] public string MockUpName;
        [JsonIgnore] public int Width;
        [JsonIgnore] public int Height;
        [JsonIgnore] public char[] Symbols;
        [JsonIgnore] public string[] FGs;
        [JsonIgnore] public string[] BGs;
        public List<int?> Features;
        [JsonIgnore] public Dictionary<Resource, int>[]? Ingredients;
        [JsonIgnore] public Dictionary<Resource, float>[]? Harvests;
        [JsonIgnore] public Research[] Researches;
        [JsonIgnore] public Research[] RequiresResearch;
        [JsonIgnore] public Type[] RequiresStructures;
        [JsonIgnore] public Resource[] StoresResources;
        public List<Coord> Squares;
        [JsonIgnore] public string UseHint;

        [JsonIgnore]
        public ResearchTask? ResearchTask
        {
            get
            {
                foreach (int? eid in Features)
                {
                    Feature? f = GetEntity<Feature>(eid);
                    if (f is null)
                    {
                        continue;
                    }
                    var (x, y, z) = f.GetPlacedCoordinate() ;
                    Task? t = Tasks.GetWithBoundsChecked(x, y, z);
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
            Features = new List<int?>();
            AddListener<TurnBeginEvent>(OnTurnBegin);
            AddListener<DespawnEvent>(OnDespawn);
            RequiresResearch = new Research[0];
            RequiresStructures = new Type[0];
            StoresResources = new Resource[0];
            Researches = new Research[0];
        }

        public void InitializeFeatures()
        {
            for (int i = 0; i < Width * Height; i++)
            {
                Features.Add(null);
            }
        }

        public static List<Type> ListStructureTypes()
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
            List<Type> s = list.Select(e => e.GetType()).ToList();
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
            List<Structure> s = list.Select(e => (Structure)e).ToList();
            return s;
        }

        public Dictionary<Resource, int> GetIngredients()
        {
            var ingredients = new Dictionary<Resource, int>();
            foreach (var resources in Ingredients ?? new Dictionary<Resource, int>[0])
            {
                if (resources != null)
                {
                    foreach (Resource resource in resources.Keys)
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

            if (StoresResources.Length > 0)
            {
                List<Item> items = Items.OrderBy((Item item) => (Tiles.Distance((int)item.X!, (int)item.Y!, (int)item.Z!, (int)X!, (int)Y!, (int)Z!))).ToList();
                foreach (Resource resource in StoresResources)
                {
                    //if there is no current haul task for that resource (fetch them one at a time)
                    if (GetHaulTask(resource) == null)
                        {
                            // loop through available items in order of distance
                            foreach (Item item in items)
                            {
                                if (item.Resource == resource && !item.IsStored() && !item.Disowned && item.Unclaimed > 0 && !item.IsHauled())
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
        public HaulTask? GetHaulTask(Resource resource)
        {
            foreach (int? eid in Features)
            {
                var f = GetEntity<Feature>(eid);
                if (f is null)
                {
                    continue;
                }
                var (x, y, z) = f.GetPlacedCoordinate();
                HaulTask? ht = Tasks.GetWithBoundsChecked(x, y, z) as HaulTask;
                if (ht != null && ht.Ingredients.ContainsKey(resource))
                {
                    return ht;
                }
            }
            return null;
        }

        public void TryToSpawnHaulTask(Item item)
        {
            if (!item.Placed || !item.Spawned)
            {
                return;
            }
            Resource resource = item.Resource!;
            List<int> order = new List<int>();
            for (int i = 0; i < Width * Height; i++)
            {
                order.Add(i);
            }
            order = order.OrderBy(s => GameState.World!.Random.NextDouble()).ToList();
            // if there is an existing pile
            foreach (int i in order)
            {
                Feature? f = GetEntity<Feature>(Features[i]);
                if (f is null || !f.Spawned || !f.Placed)
                {
                    continue;
                }
                var (x, y, z) = f.GetPlacedCoordinate();
                Item? pile = Items.GetWithBoundsChecked(x, y, z);
                Task? task = Tasks.GetWithBoundsChecked(x, y, z); //unlikely to be a haul task, should be some incidental task
                if (pile != null && pile.Resource == resource && pile.N < pile.StackSize && task == null)
                {
                    HaulTask ht = Entity.Spawn<HaulTask>();
                    ht.Structure = this.GetHandle<Structure>(ht.OnDespawn);
                    ht.PlaceInValidEmptyTile(x, y, z);
                    // this logic seems correct
                    int claim = Math.Min(item.Unclaimed, pile.StackSize - pile.N);
                    ht.Ingredients[resource] = claim;
                    ht.Resource = resource;
                    ht.Claims[(int)item.EID!] = claim;
                    // experimental code (the experiment is to set this to false)
                    //if (Options.HaulTaskClaims)
                    //{
                    //    item.Claimed += claim;
                    //}
                    return;
                }
            }
            // if there's no existing pile, repeat almost the same loop
            foreach (int i in order)
            {
                Feature? f = GetEntity<Feature>(Features[i]);
                if (f is null || !f.Placed)
                {
                    return;
                }
                var (x, y, z) = f.GetPlacedCoordinate();
                Item? pile = Items.GetWithBoundsChecked(x, y, z);
                Task? task = Tasks.GetWithBoundsChecked(x, y, z); //unlikely to be a haul task, should be some incidental task
                if (pile == null && task == null)
                {
                    HaulTask ht = Entity.Spawn<HaulTask>();
                    ht.Structure = this.GetHandle<Structure>(ht.OnDespawn);
                    ht.PlaceInValidEmptyTile(x, y, z);
                    // this logic seems correct
                    int claim = item.Unclaimed; ;
                    ht.Ingredients[resource] = claim;
                    ht.Resource = resource;
                    ht.Claims[(int)item.EID!] = claim;
                    //if (Options.HaulTaskClaims)
                    //{
                    //    item.Claimed += claim;
                    //}
                    return;
                }
            }
        }

        public virtual void BuildInfoDisplay(InfoDisplayControls menu)
        {
            if (!Placed || !Spawned)
            {
                return;
            }
            menu.SelectedEntity = this;
            // this is kind of weird...in BuildMenu we check for a task, in FinishMenu we instead check for Researching
            if (Tasks.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!) != null)
            {
                // ideally this should have the option to cancel research
                menu.Choices = new List<IMenuListable>();
            }
            else
            {
                var list = new List<IMenuListable>();
                var researched = GetState<ResearchHandler>().Researched;
                foreach (Research res in Researches)
                {
                    bool valid = true;
                    foreach (Research pre in res.RequiresResearch)
                    {
                        if (!researched.Contains(pre))
                        {
                            valid = false;
                        }
                    }
                    if (valid && !researched.Contains(res))
                    {
                        ResearchTask rt = Entity.Mock<ResearchTask>();
                        rt.Researching = res;
                        //rt.MenuName = "research " + research.Name;
                        rt.LaborCost = res.Turns;
                        rt.Ingredients = new JsonArrayDictionary<Resource, int>(res.Ingredients);
                        rt.Structure = this.GetHandle<Structure>(rt.OnDespawn);
                        list.Add(rt);
                    }
                }
                menu.Choices = list;
            }
        }

        // we are passed Cancel, , (blank header), (choices)
        public virtual void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.InfoTop.Insert(2, "Tab) Next structure.");
            menu.InfoTop.Insert(3, " ");
            menu.InfoTop.Insert(4, "{yellow}" + Describe(capitalized: true));
            menu.InfoTop.Insert(5, "{light cyan}" + UseHint);
            if (ResearchTask != null)
            {
                ResearchTask rt = ResearchTask;
                menu.InfoTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    " ",
                    "Tab) Next structure.",
                    " ",
                    "{yellow}" + Describe(capitalized: true),
                    "{light cyan}" + UseHint,
                    " ",
                    "Researching " + rt.Researching.Name + " (" + rt.Labor + " turns.)",
                    " ",
                    "Bksp/Del) Cancel research."
                };
                if (rt.Ingredients.Count > 0 && !HecatombOptions.NoIngredients)
                {
                    menu.InfoTop[7] = "Researching " + rt.Researching.Name + " ($: " + Resource.Format(rt.Ingredients) + ")";
                }
            }
            if (StoresResources.Length > 0)
            {
                var stored = GetStored();
                if (stored.Count > 0)
                {
                    menu.InfoTop.Add(" ");
                    menu.InfoTop.Add("Stored:");
                }
                foreach (Resource r in stored.Keys)
                {
                    menu.InfoTop.Add("{" + r.TextColor + "}- " + Resource.Format((r, stored[r])));
                }
            }
            menu.KeyMap[Keys.Escape] = InterfaceState.ResetControls;
            menu.KeyMap[Keys.Tab] = NextStructure;
            menu.KeyMap[Keys.Delete] = CancelResearch;
            menu.KeyMap[Keys.Back] = CancelResearch;
            menu.KeyMap[Keys.U] = Commands.ShowStructures;
            menu.KeyMap[Keys.M] = Commands.ShowMinions;
        }

        public void CancelResearch()
        {
            if (ResearchTask != null)
            {
                if (Placed && Spawned)
                {
                    Task? t = Tasks.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
                    t?.Cancel();
                    ResearchTask = null;
                } 
            }
            InterfaceState.DirtifyTextPanels();
        }

        public Dictionary<Resource, int> GetStored()
        {
            var stored = new Dictionary<Resource, int>();
            if (StoresResources.Length > 0)
            {
                foreach (int? eid in Features)
                {
                    Feature? f = GetEntity<Feature>(eid);
                    if (f is null)
                    {
                        continue;
                    }
                    var (x, y, z) = f.GetPlacedCoordinate();
                    Item? item = Items.GetWithBoundsChecked(x, y, z);
                    if (item != null && StoresResources.Contains(item.Resource))
                    {
                        if (stored.ContainsKey(item.Resource!))
                        {
                            stored[item.Resource!] += item.N;
                        }
                        else
                        {
                            stored[item.Resource!] = item.N;
                        }
                    }
                }
            }
            return stored;
        }

        public void NextStructure()
        {
            var structures = ListStructures();
            if (structures.Count > 0)
            {
                int n = -1;
                for (int i = 0; i < structures.Count; i++)
                {
                    if (structures[i] == this)
                    {
                        n = i;
                    }
                }
                if (n == -1 || n == structures.Count - 1)
                {
                    //ControlContext.Set(new MenuChoiceControls((Structure)structures[0]));
                    InterfaceState.SetControls(new InfoDisplayControls((Structure)structures[0]));
                    InterfaceState.Camera!.CenterOnSelection();
                }
                else
                {
                    //ControlContext.Set(new MenuChoiceControls((Structure)structures[n+1]));
                    InterfaceState.SetControls(new InfoDisplayControls((Structure)structures[n + 1]));
                    InterfaceState.Camera!.CenterOnSelection();
                }
            }
            else
            {
                InterfaceState.Camera!.CenterOnSelection();
            }
        }
        public void BuildInSquares(List<Coord> squares)
        {
            Squares = squares;
            BuildInSquares();
        }
        public void BuildInSquares()
        {
            //Structure existingStructure;
            for (int i = 0; i < Squares.Count; i++)
            {
                Coord s = Squares[i];
                Feature? f = GameState.World!.Features.GetWithBoundsChecked(s.X, s.Y, s.Z);
                if (Tasks.GetWithBoundsChecked(s.X, s.Y, s.Z) != null)
                {
                    return;
                }
                else if (f != null)
                {
                    if (f is IncompleteFixture && (f as IncompleteFixture)!.Structure?.UnboxBriefly() == this)
                    {
                        // this is okay, go ahead
                    }
                    else if (f is StructuralFeature && (f as StructuralFeature)!.Structure?.UnboxBriefly() == this)
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
                if (Tasks.GetWithBoundsChecked(s.X, s.Y, s.Z) is null)
                {
                    Feature? f = GameState.World!.Features.GetWithBoundsChecked(s.X, s.Y, s.Z);
                    if (f is null || f is IncompleteFixture)
                    {
                        ConstructTask tc = Entity.Spawn<ConstructTask>();
                        tc.Makes = this.GetType();
                        tc.Structure = this.GetHandle<Structure>(tc.OnDespawn);
                        tc.FeatureIndex = i;
                        tc.Ingredients =  new JsonArrayDictionary<Resource, int>(Ingredients?[i] ?? new Dictionary<Resource, int>());
                        tc.Harvests = new JsonArrayDictionary<Resource, float>(Harvests?[i] ?? new Dictionary<Resource, float>());
                        tc.PlaceInValidEmptyTile(s.X, s.Y, s.Z);
                    }
                    else if (f is StructuralFeature)
                    {
                        var sf = (StructuralFeature)f;
                        if (sf.Structure?.UnboxBriefly() != this)
                        {
                            throw (new InvalidOperationException("You're trying to build one structure on top of another!"));
                        }
                        else
                        {
                            //Defender d = f.GetComponent<Defender>();
                            //if (d.Wounds > 0)
                            //{
                            //    RepairTask rt = Entity.Spawn<RepairTask>();
                            //    rt.Ingredients = Ingredients[i] ?? new Dictionary<string, int>();
                            //    rt.Place(s.X, s.Y, s.Z);
                            //}
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
                    Feature? f = GameState.World!.Features.GetWithBoundsChecked(s.X, s.Y, s.Z);
                    if (f != null && f != despawning)
                    {
                        if (f is StructuralFeature && (f as StructuralFeature)!.Structure?.UnboxBriefly() == this) 
                        {
                            exists = true;
                        }
                        else if (f is IncompleteFixture && (f as IncompleteFixture)!.Structure?.UnboxBriefly() == this)
                        {
                            exists = true;
                        }
                    }
                    Task? t = Tasks.GetWithBoundsChecked(s.X, s.Y, s.Z);
                    if (t != null && t != despawning && t is ConstructTask)
                    {
                        if (t.Makes == this.GetType())
                        {
                            exists = true;
                        }
                    }
                }
                return exists;
            }
        }
        public virtual GameEvent OnDespawn(GameEvent ge)
        {

            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity is Feature)
            {
                Feature f = (Feature)de.Entity;
                if (!f.Spawned)
                {
                    return ge;
                }
                if (Placed && Features.Contains(f.EID))
                {
                    CancelResearch();
                    Remove();
                    // This code is kind of experimental, but it seems to work
                    Features[Features.IndexOf(f.EID)] = null;
                    if (!AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                }
                if (!Placed)
                {
                    var ifc = (f as IncompleteFixture);
                    if (ifc != null && ifc.Structure?.UnboxBriefly() == this && !AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                    var sc = (f as StructuralFeature);
                    if (sc != null && sc.Structure?.UnboxBriefly() == this && !AtLeastPartiallyExists(f))
                    {
                        Despawn();
                    }
                }
            }
            if (de.Entity is ConstructTask)
            {
                if (!AtLeastPartiallyExists((ConstructTask)de.Entity))
                {
                    Despawn();
                }
            }
            return ge;
        }

        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            base.PlaceInValidEmptyTile(x, y, z);
            if (Spawned)
            {
                Publish(new AfterPlaceEvent() { Entity = this, X = x, Y = y, Z = z });
            }
        }
    }
}
