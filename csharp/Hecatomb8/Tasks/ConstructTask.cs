using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class ConstructTask : Task, IDisplayInfo, ISelectsBox
    {
        public int FeatureIndex;
        public JsonArrayDictionary<Resource, float> Harvests;
        [JsonIgnore]
        public new int BoxWidth
        {
            get
            {
                return Mock().Width;
            }
            set { }
        }

        [JsonIgnore]
        public new int BoxHeight
        {
            get
            {
                return Mock().Height;
            }
            set { }
        }
        public ListenerHandledEntityHandle<Structure> Structure;
        public static Type[] Structures = new Type[] { typeof(Workshop)};

        protected List<IMenuListable> cachedChoices;
        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            if (cachedChoices != null)
            {
                menu.Choices = cachedChoices;
                return;
            }
            var list = new List<IMenuListable>();
            var structures = Hecatomb8.Structure.ListStructureTypes();
            var researched = GetState<ResearchHandler>().Researched;
            foreach (Type st in Structures)
            {
                var structure = (Structure)Hecatomb8.Entity.Mock(st);
                bool valid = true;
                foreach (Research r in structure.RequiresResearch)
                {
                    if (!HecatombOptions.NoIngredients && !researched.Contains(r))
                    {
                        valid = false;
                    }
                }
                foreach (Type s in structure.RequiresStructures)
                {
                    if (!HecatombOptions.NoIngredients && !structures.Contains(s))
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    var task = Hecatomb8.Entity.Mock<ConstructTask>();
                    task.Makes = st;
                    task.Ingredients = new JsonArrayDictionary<Resource, int>(structure.GetIngredients());
                    list.Add(task);
                }
            }
            //var repair = Hecatomb.Entity.Mock<RepairTask>();
            //repair.MenuName = "repair or complete structure";
            //list.Add(repair);
            cachedChoices = list;
            menu.Choices = list;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {

        }

        public ConstructTask() : base()
        {
            //Structure = new TileEntityField<Structure>();
            //Structures = new string[] { "GuardPost", "Workshop", "Stockpile", "Slaughterhouse", "Sanctum", "BlackMarket", "StoneMason", "Forge", "Chirurgeon", "Apothecary", "Library", "Treasury" };
            _name = "construct or repair a structure";
            Harvests = new JsonArrayDictionary<Resource, float>();
            Priority = 4;
            LaborCost = 5;
            Labor = 5;
            _bg = "yellow";
        }

        protected override string getName()
        {
            if (Structure?.UnboxBriefly() is null)
            {
                return _name!;
            }
            return $"construct {Structure.UnboxBriefly()!.Name}";
        }

        private Structure Mock()
        {   
            // we pretty much have to assume this isn't null; I can't think of any fallback
            if (Makes is null)
            {
                return (Structure)Hecatomb8.Entity.Mock(typeof(Workshop));
            }
            return (Structure)Hecatomb8.Entity.Mock(Makes!);
        }

        public override bool CanAssign(Creature c)
        {
            // don't start building the "free" structure tiles until the "costly" ones are done
            if (Ingredients.Count == 0)
            {
                foreach (Task task in Tasks)
                {
                    if (task is ConstructTask)
                    {
                        ConstructTask ct = (ConstructTask)task;
                        if (ct.Structure == Structure && ct.Worker == null && ct.Ingredients.Count > 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return base.CanAssign(c);
        }

        public override void Start()
        {
            base.Start();
            if (!Placed || !Spawned | Structure?.UnboxBriefly() is null)
            {
                return;
            }
            Feature? f = Features.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
            if (f is IncompleteFixture)
            {
                var ifx = (IncompleteFixture)f;
                ifx.IncompleteFG = Structure!.UnboxBriefly()!.FGs[FeatureIndex];
                ifx.Makes = Makes;
                // pass it a fake handler...hmmm...what would happen if part of the completed structure got destroyed before all of it was built?
                ifx.Structure = this.Structure.UnboxBriefly()!.GetHandle<Structure>(ifx.OnDespawn);
            }
        }
        public override void Finish()
        {
            if (!Placed || !Spawned || Structure?.UnboxBriefly() is null)
            {
                return;
            }
            var (x, y, z) = GetVerifiedCoord();
            Publish(new TutorialEvent() { Action = "AnyBuildComplete" });
            Structure s = Structure!.UnboxBriefly()!;
            if (s.Features.Count == 0)
            {
                for (int i = 0; i < s.Width * s.Height; i++)
                {
                    s.Features.Add(null);
                }
            }
            Feature? inf = Features.GetWithBoundsChecked(x, y, z);
            var ifx = (inf as IncompleteFixture);
            if (inf is null || ifx is null)
            {
                return;
            }
            inf!.Despawn();
            StructuralFeature f = Spawn<StructuralFeature>();
            f.PlaceInValidEmptyTile(x, y, z);
            Cover.ClearGroundCover(x, y, z);
            f.StructuralSymbol = s.Symbols[FeatureIndex];
            f.StructuralName = Structure.UnboxBriefly()!.Name;
            f.StructuralFG = s.FGs[FeatureIndex];
            //f.BG = sc.BGs[FeatureIndex];
            f.StructuralBG = s.BG;
            s.Features[FeatureIndex] = f.EID;
            // this is weird, shouldn't they just be a default part of it?
            f.Structure = Structure.UnboxBriefly()!.GetHandle<Structure>(f.OnDespawn);
            Harvestable h = Spawn<Harvestable>();
            h.Yields = new JsonArrayDictionary<Resource, float>(Harvests);
            h.AddToEntity(f);
            bool finished = true;
            foreach (int? eid in s.Features)
            {
                Feature? fr = GetEntity<Feature>(eid);
                if (fr == null)
                {
                    finished = false;
                }
            }
            if (finished)
            {
                Feature fr = GetEntity<Feature>(s.Features[0])!;
                if (s.Width == 3 && s.Height == 3)
                {
                    fr = GetEntity<Feature>(Structure!.UnboxBriefly()!.Features[4])!;
                }
                else if (s.Width == 4 && s.Height == 4)
                {
                    fr = GetEntity<Feature>(Structure!.UnboxBriefly()!.Features[5])!;
                }
                Structure!.UnboxBriefly()!.PlaceInValidEmptyTile((int)fr.X!,(int)fr.Y!, (int)fr.Z!);
                //foreach (Feature feat in s.Features)
                //{
                //   StructuralComponent st = Spawn<StructuralComponent>();
                //   st.Structure = Structure;
                //   st.AddToEntity(feat);
                //}
            }
            base.Finish();
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            if (Makes == null)
            {
                var menu = new InfoDisplayControls(this);
                menu.Header = "Construct a structure:";
                menu.MenuCommandsSelectable = false;
                menu.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(menu);
            }
            else
            {
                var c = new SelectBoxControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(c);
            }
        }

        public override void BoxHover(Coord c, List<Coord> squares)
        {
            base.BoxHover(c, squares);
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Build {0} in this area.", Mock().Name) };
        }

        public override void SelectBox(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "ConstructTask", squares: squares, makes: Makes!.Name);
            foreach (Coord s in squares)
            {
                Feature? f = Features.GetWithBoundsChecked(s.X, s.Y, s.Z);
                Task? t = Tasks.GetWithBoundsChecked(s.X, s.Y, s.Z);
                if (t == null && f != null)
                {
                    if (f is IncompleteFixture && (f as IncompleteFixture)!.Makes == Makes)
                    {
                        var ifx = (IncompleteFixture)f;
                        if (ifx.Structure?.UnboxBriefly() is null)
                        {
                            return;
                        }
                        Structure st = ifx.Structure!.UnboxBriefly()!;
                        st.BuildInSquares(st.Squares);
                        return;
                    }
                    else if (f is StructuralFeature)
                    {
                        Structure st = (f as StructuralFeature)!.Structure!.UnboxBriefly()!;
                        if (st.GetType() == Makes && !st.Placed)
                        {
                            st.BuildInSquares(st.Squares);
                            return;
                        }
                        if (st.Placed && st.GetType() == Makes)
                        {
                            // call up repairs if need be
                            st.BuildInSquares(st.Squares);
                            return;
                        }
                    }
                }
                if (!ValidTile(s))
                {
                    return;
                }
            }
            Structure str = Spawn<Structure>(Makes);
            str.BuildInSquares(squares);
        }
    }

}
