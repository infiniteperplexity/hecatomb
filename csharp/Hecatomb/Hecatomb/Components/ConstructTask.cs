/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:40 PM
 */
using System;

/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	public class ConstructTask : Task, IChoiceMenu
	{
		public string[] Structures;
		public string MenuHeader
		{
			get
			{
				return "Choose a structure:";
			}
			set {}
		}
		public List<IMenuListable> MenuChoices
		{
			get
			{
				return new List<IMenuListable>();
			}
			set {}
		}
		
		public ConstructTask(): base()
		{
			Structures = new string[]{};
			MenuName = "construct a structure.";
		}
			
		public override void Start()
		{
//			base.Start();
//			Feature f = Game.World.Features[Entity.x, Entity.y, Entity.z];
//			f.Symbol = '\u2717';
//			f.FG = "white";
		}
		public override void Finish()
		{
			Complete();
		}
		
		public override void ChooseFromMenu()
		{
//			Game.Controls.Set(new MenuChoiceControls(this));
		}
		
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<string>() {String.Format("Dig from {0} {1} {2}", c.x, c.y, c.z)};
			co.MiddleColors[0,0] = "green";
		}
		public override void TileHover(Coord c, List<Coord> squares)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<string>() {String.Format("Dig to {0} {1} {2}", c.x, c.y, c.z)};
			co.MiddleColors[0,0] = "red";
		}
	}

}
