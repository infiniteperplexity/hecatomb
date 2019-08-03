﻿
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

        public override string GetDisplayName()
        {
            Feature f = Entity.Mock<Feature>(Makes);
            return $"furnish {f.Name}";
        }

        public void BuildMenu(MenuChoiceControls menu)
        {
            menu.Header = "Choose a fixture:";
            var list = new List<IMenuListable>();
            var structures = Structure.ListAsStrings();
            var researched = Game.World.GetState<ResearchHandler>().Researched;
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
                if (valid || Game.Options.NoIngredients)
                {
                    var task = Hecatomb.Entity.Mock<FurnishTask>();
                    var feat = Entity.Mock<Feature>(f);
                    task.Makes = f;
                    task.MenuName = "furnish " + feat.Name;
                    list.Add(task);
                }
            }
            //list.Add(Hecatomb.Entity.Mock<RepairTask>());
            menu.Choices = list;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }
       

        public FurnishTask(): base()
		{
			MenuName = "build or repair a fixture";
            Priority = 4;
            Fixtures = new string[] {"Door", "TiledFloor", "Ramp", "SpearTrap" };
            PrereqStructures = new List<string> { "Workshop" };
            BG = "yellow";
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
		
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Build {3} at {0} {1} {2}", c.X, c.Y, c.Z, Makes)};
		}

        public override void SelectTile(Coord c)
        {
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
            {
                Feature f = Game.World.Features[c];
                Defender d = f?.TryComponent<Defender>();
                if (f != null && d != null && f.TypeName == Makes && d.Wounds > 0)
                {
                    Task task = Entity.Spawn<RepairTask>();
                    string json = EntityType.Types[Makes].Components["Fixture"];
                    JObject obj = JObject.Parse(json);
                    var ingredients = obj["Ingredients"];
                    task.Ingredients = (ingredients == null) ? new Dictionary<string, int>() : ingredients.ToObject<Dictionary<string, int>>();
                    task.Place(c.X, c.Y, c.Z);
                }
                else
                {
                    Task task = Entity.Spawn<FurnishTask>();
                    string json = EntityType.Types[Makes].Components["Fixture"];
                    JObject obj = JObject.Parse(json);
                    var ingredients = obj["Ingredients"];

                    task.Ingredients = (ingredients == null) ? new Dictionary<string, int>() : ingredients.ToObject<Dictionary<string, int>>();
                    task.Makes = Makes;
                    task.Place(c.X, c.Y, c.Z);
                }       
            }
        }

        public override bool ValidTile(Coord c)
        {
            Feature f = Game.World.Features[c];
           Defender d = f?.TryComponent<Defender>();
            if (f != null && d != null && f.TypeName == Makes && d.Wounds > 0)
            {
                return true;
            }
            return base.ValidTile(c);
        }
    }

}

