/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:40 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;

namespace Hecatomb
{
    using static HecatombAliases;

    public class ConstructTask : Task, IChoiceMenu, ISelectsBox
	{
		public int FeatureIndex;
        public Dictionary<string, float> Harvests;
		[JsonIgnore] public new int BoxWidth
		{
			get
			{
				return Mock().Width;
			}
			set {}
		}

        [JsonIgnore]
        public new int BoxHeight
		{
			get
			{
				return Mock().Height;
			}
			set {}
		}
        public TileEntityField<Structure> Structure;
		public string[] Structures;
        public void BuildMenu(MenuChoiceControls menu)
        {
            var list = new List<IMenuListable>();
            var structures = Hecatomb.Structure.ListAsStrings();
            var researched = Game.World.GetState<ResearchHandler>().Researched;
            foreach (string st in Structures)
            {
                var structure = (Structure)Hecatomb.Entity.Mock(Type.GetType("Hecatomb." + st));
                bool valid = true;
                foreach (string s in structure.ResearchPrereqs)
                {
                    if (!researched.Contains(s))
                    {
                        valid = false;
                    }
                }
                foreach (string s in structure.StructurePrereqs)
                {
                    if (!structures.Contains(s))
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    var task = Hecatomb.Entity.Mock<ConstructTask>();
                    task.Makes = st;
                    task.MenuName = "construct " + structure.Describe();
                    task.Ingredients = structure.GetIngredients();
                    list.Add(task);
                }
            }
            //var repair = Hecatomb.Entity.Mock<RepairTask>();
            //repair.MenuName = "repair or complete structure";
            //list.Add(repair);
            menu.Choices = list;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }

        public ConstructTask(): base()
		{
            Structure = new TileEntityField<Structure>();
			Structures = new string[]{"GuardPost", "Workshop","Stockpile","Slaughterhouse","Sanctum", "BlackMarket", "StoneMason", "Forge", "Chirurgeon", "Library", "Treasury"};
			MenuName = "construct or repair a structure";
            Harvests = new Dictionary<string, float>();
            Priority = 4;
            LaborCost = 5;
            Labor = 5;
            BG = "yellow";
		}

        public override string GetDisplayName()
        {
            return $"construct {Structure.Name}";
        }

        private Structure Mock()
		{
			return (Structure) Hecatomb.Entity.Mock(Type.GetType("Hecatomb."+Makes));
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
            Feature f = Game.World.Features[X, Y, Z];
			f.FG = Structure.Entity.FGs[FeatureIndex];
            f.Name = "incomplete " + Structure.Name;
            f.GetComponent<IncompleteFixtureComponent>().Structure = Structure;
        }
		public override void Finish()
		{
            Structure s = Structure.Entity;
            if (s.Features.Count==0)
            {
                for (int i = 0; i < s.Width * s.Height; i++)
                {
                    s.Features.Add(null);
                }
            }
			Feature incomplete = Game.World.Features[X, Y, Z];
            var x = incomplete.GetComponent<IncompleteFixtureComponent>();
            incomplete.Despawn();
			Feature f = Entity.Spawn<Feature>("StructureFeature");
			f.Place(X, Y, Z);
            Game.World.Covers[X, Y, Z] = Cover.NoCover;
			f.Symbol = s.Symbols[FeatureIndex];
            f.Name = Structure.Name;
			f.FG = s.FGs[FeatureIndex];
            //f.BG = sc.BGs[FeatureIndex];
            f.BG = s.BG;
			s.Features[FeatureIndex] = f;
            // this is weird, shouldn't they just be a default part of it?
            StructuralComponent st = Spawn<StructuralComponent>();
            st.Structure = Structure;
            st.AddToEntity(f);
            Harvestable h = Spawn<Harvestable>();
            h.Yields = (Harvests != null) ? new Dictionary<string, float>(Harvests) : new Dictionary<string, float>();
            h.AddToEntity(f);
            bool finished = true;
			foreach (Feature fr in s.Features)
			{
				if (fr==null)
				{
					finished = false;
				}
			}
			if (finished)
			{
                Feature fr = s.Features[0];
                if (s.Width==3 && s.Height==3)
                {
                    fr = Structure.Entity.Features[4];
                }
                else if (s.Width==4 && s.Height==4)
                {
                    fr = Structure.Entity.Features[5];
                }
				Structure.Entity.Place(fr.X, fr.Y, fr.Z);
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
			if (Makes==null)
			{
				Game.Controls.Set(new MenuChoiceControls(this));
			}
			else
			{
				Game.Controls.Set(new SelectBoxControls(this));
			}
		}

        public override void TileHover(Coord c)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<ColoredText>() {"{green}"+String.Format("Build from {0} {1} {2}", c.X, c.Y, c.Z)};
		}
		
		public override void SelectBox(Coord c, List<Coord> squares)
		{
            foreach (Coord s in squares)
            {
                Feature f = Game.World.Features[s.X, s.Y, s.Z];
                Task t = Game.World.Tasks[s.X, s.Y, s.Z];
                if (t == null && f != null)
                {
                    if (f.TryComponent<IncompleteFixtureComponent>() != null && f.GetComponent<IncompleteFixtureComponent>().Makes == Makes)
                    {
                        Structure st = f.GetComponent<IncompleteFixtureComponent>().Structure;
                        st.BuildInSquares(st.Squares);
                        return;
                    }
                    else if (f.TryComponent<StructuralComponent>() != null)
                    {
                        Structure st = f.GetComponent<StructuralComponent>().Structure;
                        if (st.GetType().Name == Makes && !st.Placed)
                        {
                            st.BuildInSquares(st.Squares);
                            return;
                        }
                        if (st.Placed && st.GetType().Name == Makes)
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
			Structure str = Spawn<Structure>(Type.GetType("Hecatomb."+Makes));
            str.BuildInSquares(squares);
		}
	}

}
