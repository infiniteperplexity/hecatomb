/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 1:03 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of UndesignateTask.
	/// </summary>
	public class UndesignateTask : Task, ISelectsZone, IMenuListable
	{
		public UndesignateTask() : base()
		{
			MenuName = "cancel tasks";
		}
		
		public override void ChooseFromMenu()
		{
			Game.Controls.Set(new SelectZoneControls(this));
		}
			
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Undesignate from {0} {1} {2}", c.X, c.Y, c.Z)};
		}
		public override void TileHover(Coord c, List<Coord> squares)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Undesignate to {0} {1} {2}", c.X, c.Y, c.Z)};
		}
		
		public override void SelectZone(List<Coord> squares)
		{
			foreach (Coord c in squares)
			{
				var te = Game.World.Tasks[c.X, c.Y, c.Z];
				if (te!=null) 
				{
					te.GetComponent<Task>().Cancel();
				}
			}
		}
	}
}
