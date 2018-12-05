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

namespace Hecatomb
{
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
		[JsonProperty] private int StructureEID;
		[JsonIgnore] public StructureEntity Structure
		{
			get
			{
				return (StructureEntity) Game.World.Entities.Spawned[StructureEID];
			}
			set
			{
				StructureEID = value.EID;
			}
		}
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
                var structures = Hecatomb.Structure.ListAll();
                var researched = Game.World.GetTracker<ResearchTracker>().Researched;
                foreach (string st in Structures)
				{   
                    var structure = (Structure) Game.World.Entities.Mock(Type.GetType("Hecatomb."+st));
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
                        var task = Game.World.Entities.Mock<ConstructTask>();
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
			Structures = new string[]{"GuardPost", "Workshop","Stockpile","BlackMarket","Sanctum"};
			MenuName = "construct a structure.";
			TypeName = "construct";
		}
			
		private Structure Mock()
		{
			return (Structure) Game.World.Entities.Mock(Type.GetType("Hecatomb."+Makes));
		}
		
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			f.FG = Structure.GetComponent<Structure>().FGs[FeatureIndex];
		}
		public override void Finish()
		{
			Feature incomplete = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			incomplete.Despawn();
			Feature f = Game.World.Entities.Spawn<Feature>(Makes+"Feature");
			f.Place(Entity.X, Entity.Y, Entity.Z);
			Structure sc = Structure.GetComponent<Structure>();
			f.Symbol = sc.Symbols[FeatureIndex];
			f.FG = sc.FGs[FeatureIndex];
            //f.BG = sc.BGs[FeatureIndex];
            f.BG = sc.BG;
			sc.Features[FeatureIndex] = f;
			bool finished = true;
			foreach (Feature fr in sc.Features)
			{
				if (fr==null)
				{
					finished = false;
				}
			}
			if (finished)
			{
				Feature fr = sc.Features[0];
                Game.StatusPanel.PushMessage(String.Format("{0} {1} {2}", fr.X, fr.Y, fr.Z));
				Structure.Place(fr.X, fr.Y, fr.Z);
                foreach (Feature feat in sc.Features)
                {
                    Structural st = Game.World.Entities.Spawn<Structural>();
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
                if (structure.GetIngredients().Count==0 /*|| Game.Options.NoIngredients*/)
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
			StructureEntity str = Game.World.Entities.Spawn<StructureEntity>(Makes);
			for (int i=0; i<squares.Count; i++)
			{
				Coord s = squares[i];
				if (Game.World.Tasks[s.X, s.Y, s.Z]==null) 
				{
					TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
					ConstructTask tc = (ConstructTask) task.GetComponent<Task>();
					tc.Makes = Makes;
					tc.Structure = str;
					tc.FeatureIndex = i;
                    tc.Ingredients = str.GetComponent<Structure>().Ingredients[i] ?? new Dictionary<string, int>();
                    task.Place(s.X, s.Y, s.Z);
				}
			}
		}
	}

}
