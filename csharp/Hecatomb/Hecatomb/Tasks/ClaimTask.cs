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
    public class ClaimTask : Task, ISelectsZone, IMenuListable
    {
        public ClaimTask() : base()
        {
            MenuName = "claim items";
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectZoneControls(this));
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Claim items from {0} {1} {2}", c.X, c.Y, c.Z) };
        }
        public override void TileHover(Coord c, List<Coord> squares)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Claim items to {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override void SelectZone(List<Coord> squares)
        {
            foreach (Coord c in squares)
            {
                if (Items[c.X, c.Y, c.Z] != null)
                {
                    Items[c.X, c.Y, c.Z].Owned = true;
                }
            }
        }
    }
}
