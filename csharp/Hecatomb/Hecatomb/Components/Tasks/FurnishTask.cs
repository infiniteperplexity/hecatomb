
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
				foreach (string s in Fixtures)
				{
					var task = Game.World.Entities.Mock<FurnishTask>();
					task.Makes = s;
					list.Add(task);
				}
				return list;
			}
			set {}
		}
        [JsonIgnore]
        public List<bool> ValidChoices
        {
            get
            {
                List<bool> list = new List<bool>();
                foreach (string s in Fixtures)
                {
                    list.Add(true);
                }
                return list;
            }
            set { }
        }
        //		public static 

        public FurnishTask(): base()
		{
			MenuName = "build fixtures";
			TypeName = "furnish";
			Fixtures = new string[] {"Ramp", "Door"};
		}
			
		public override void Finish()
		{
			Feature incomplete = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			incomplete.Despawn();
			Feature finished = Game.World.Entities.Spawn<Feature>(Makes);
			finished.Place(Entity.X, Entity.Y, Entity.Z);
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
		
		public override string ListOnMenu()
		{
			if (Makes!=null)
			{
				return Makes;
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
                TaskEntity task = Game.World.Entities.Spawn<TaskEntity>(this.GetType().Name);
                string json = EntityType.Types[Makes].Components["Fixture"];
                JObject obj = JObject.Parse(json);
                var ingredients = obj["Ingredients"];
                Debug.WriteLine(ingredients);
                task.GetComponent<Task>().Ingredients = ingredients.ToObject<Dictionary<string, int>>();
                Debug.WriteLine(task.GetComponent<Task>().Ingredients.Count);
                task.GetComponent<Task>().Makes = Makes;
                task.Place(c.X, c.Y, c.Z);
                
            }
        }
    }

}

