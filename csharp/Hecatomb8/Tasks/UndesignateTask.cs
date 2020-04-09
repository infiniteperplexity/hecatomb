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
    using static HecatombAliases;
	/// <summary>
	/// Description of UndesignateTask.
	/// </summary>
	public class UndesignateTask : Task, ISelectsZone, IMenuListable
	{
		public UndesignateTask() : base()
		{
			MenuName = "cancel tasks";
            BG = "#6699EE";
		}
		
		public override void ChooseFromMenu()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectZoneControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.MenuSelectable = false;
            ControlContext.Set(c);
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
		
        public override bool ValidTile(Coord c)
        {
            return true;
        }
		public override void SelectZone(List<Coord> squares)
		{
			CommandLogger.LogCommand(command: "UndesignateTask", squares: squares);
			foreach (Coord c in squares)
			{

				Tasks[c.X, c.Y, c.Z]?.Cancel();
			}
		}
	}
}
