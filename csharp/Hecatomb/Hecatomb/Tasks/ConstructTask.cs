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
        [JsonIgnore]
        public string MenuHeader
		{
			get
			{
				return "Choose a structure:";
			}
			set {}
		}
        [JsonIgnore]
        public List<IMenuListable> MenuChoices
		{
			get
			{
				var list = new List<IMenuListable>();
                var structures = Hecatomb.Structure.ListAsStrings();
                var researched = Game.World.GetState<ResearchHandler>().Researched;
                foreach (string st in Structures)
				{   
                    var structure = (Structure) Hecatomb.Entity.Mock(Type.GetType("Hecatomb."+st));
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
                        list.Add(task);
                    }
				}
				return list;
			}
			set {}
		}

        public ConstructTask(): base()
		{
            Structure = new TileEntityField<Structure>();
			Structures = new string[]{"GuardPost", "Workshop","Stockpile","Slaughterhouse","Sanctum", "BlackMarket" };
			MenuName = "construct a structure.";
            Priority = 4;
            LaborCost = 5;
            Labor = 5;
		}

        public override string GetDisplayName()
        {
            //return $"construct {Structure.Name}";
            if (Claims.Count>0)
            {
                return $"claiming {Claims.Keys.ToList()[0]}";
               
            }
            return "nothing claimed";
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
			incomplete.Despawn();
			Feature f = Entity.Spawn<Feature>("StructureFeature");
			f.Place(X, Y, Z);
            Game.World.Covers[X, Y, Z] = Cover.NoCover;
			f.Symbol = s.Symbols[FeatureIndex];
			f.FG = s.FGs[FeatureIndex];
            //f.BG = sc.BGs[FeatureIndex];
            f.BG = s.BG;
			s.Features[FeatureIndex] = f;
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
                foreach (Feature feat in s.Features)
                {
                    StructuralComponent st = Spawn<StructuralComponent>();
                    st.Structure = Structure;
                    st.AddToEntity(feat);
                }
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
		
		public override ColoredText ListOnMenu()
		{

			if (Makes!=null)
			{
                var structure = Mock();
                if (structure.GetIngredients().Count==0 || Game.Options.NoIngredients)
                {
                    return Makes;
                }
                else if (Game.World.Player.GetComponent<Movement>().CanFindResources(structure.GetIngredients()))
                {
                    return (Makes + " ($: " + Resource.Format(structure.GetIngredients()) + ")");
                }
                else
                {
                    return ("{gray}" + Makes + " ($: " + Resource.Format(structure.GetIngredients()) + ")");
                }
                
			}
			else
			{
				return base.ListOnMenu();
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
                if (!ValidTile(s))
                {
                    return;
                }
            }
			Structure str = Spawn<Structure>(Type.GetType("Hecatomb."+Makes));
			for (int i=0; i<squares.Count; i++)
			{
				Coord s = squares[i];
				if (Game.World.Tasks[s.X, s.Y, s.Z]==null) 
				{
					ConstructTask tc = Entity.Spawn<ConstructTask>();
					tc.Makes = Makes;
					tc.Structure = str;
					tc.FeatureIndex = i;
                    tc.Ingredients = str.Ingredients[i] ?? new Dictionary<string, int>();
                    tc.Place(s.X, s.Y, s.Z);
				}
			}
		}
	}

}
