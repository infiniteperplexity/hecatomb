
/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	public class FurnishTask : Task, IChoiceMenu, IMenuListable
	{
		string [] Fixtures;
		public string MenuHeader
		{
			get
			{
				return "Choose a fixture:";
			}
			set {}
		}
		public List<IMenuListable> MenuChoices
		{
			get
			{
				var list = new List<IMenuListable>();
                var structures = Structure.ListAll();
                var researched = Game.World.GetTracker<ResearchTracker>().Researched;
                Debug.WriteLine(JsonConvert.SerializeObject(researched));
                // only if we have the prerequisite structures / technologies...
                foreach (string f in Fixtures)
				{
                    var fixture = Hecatomb.Entity.Mock<Fixture>();
                    fixture.InterpretJSON(EntityType.Types[f].Components["Fixture"]);
                    bool valid = true;
                    
                    foreach (string s in fixture.Research)
                    {
                        if (!researched.Contains(s))
                        {
                            valid = false;
                        }
                    }
                    foreach (string s in fixture.Structures)
                    {
                        if (!structures.Contains(s))
                        {
                            valid = false;
                        }
                    }
                    if (valid)
                    {
                        var task = Hecatomb.Entity.Mock<FurnishTask>();
                        task.Makes = f;
                        list.Add(task);
                    }
				}
				return list;
			}
			set {}
		}
        //		public static 

        public FurnishTask(): base()
		{
			MenuName = "build fixtures";
			TypeName = "furnish";
			Fixtures = new string[] {"Ramp", "Door", "SpearTrap"};
		}
			
		public override void Finish()
		{
			Feature incomplete = Game.World.Features[X, Y, Z];
			incomplete.Despawn();
            Game.World.Covers[X, Y, Z] = Cover.NoCover;
			Feature finished = Entity.Spawn<Feature>(Makes);
			finished.Place(X, Y, Z);
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
				Game.Controls.Set(new SelectTileControls(this));
			}
		}



        public override ColoredText ListOnMenu()
		{
            
			if (Makes!=null)
			{
                var fixture = Hecatomb.Entity.Mock<Fixture>();
                fixture.InterpretJSON(EntityType.Types[Makes].Components["Fixture"]);
                if (fixture.Ingredients.Count == 0)
                {
                    return Makes;
                }
                else if (Game.World.Player.GetComponent<Movement>().CanFindResources(fixture.Ingredients))
                {
                    return (Makes + " ($: " + Resource.Format(fixture.Ingredients) + ")");
                }
                else
                {
                    return ("{gray}" + Makes + " ($: " + Resource.Format(fixture.Ingredients) + ")");
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
			co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Build {3} at {0} {1} {2}", c.X, c.Y, c.Z, Makes)};
		}

        public override void SelectTile(Coord c)
        {
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null)
            {
                Task task = Entity.Spawn<FurnishTask>();
                string json = EntityType.Types[Makes].Components["Fixture"];
                JObject obj = JObject.Parse(json);
                var ingredients = obj["Ingredients"];
                task.Ingredients = ingredients.ToObject<Dictionary<string, int>>();
                task.Makes = Makes;
                task.Place(c.X, c.Y, c.Z);
                
            }
        }
    }

}

