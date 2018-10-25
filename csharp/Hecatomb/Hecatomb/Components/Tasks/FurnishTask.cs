
/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
			incomplete.Destroy();
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
			co.MenuMiddle = new List<string>() {String.Format("Build {3} at {0} {1} {2}", c.X, c.Y, c.Z, Makes)};
			co.MiddleColors[0,0] = "green";
		}
	}

}

