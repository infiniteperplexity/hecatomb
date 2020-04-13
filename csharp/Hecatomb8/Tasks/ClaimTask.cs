/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 1:03 PM
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    /// <summary>
    /// Stupid name, this actually means declare ownership
    /// </summary>
    public class ClaimTask : Task, ISelectsZone, IMenuListable
    {
        public ClaimTask() : base()
        {
            MenuName = "toggle item claims";
            PrereqStructures = new List<string>() { "Stockpile" };
            BG = "pink";
        }

        public override void ChooseFromMenu()
        {
            var c = new SelectZoneControls(this);
            c.SelectedMenuCommand = "Jobs";
            ControlContext.Set(c);
        }

        public override void TileHover(Coord c)
        {
            var co = OldGame.Controls;
            co.MenuMiddle.Clear();
            co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Toggle item claims from {0} {1} {2}", c.X, c.Y, c.Z) };
        }
        public override void TileHover(Coord c, List<Coord> squares)
        {
            var co = OldGame.Controls;
            co.MenuMiddle.Clear();
            co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Toggle item claims to {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "ClaimTask", squares: squares);
            foreach (Coord c in squares)
            {
                var item = Items[c.X, c.Y, c.Z];
                if (item != null)
                {
                    if (item.Owned)
                    {
                        item.Owned = false;
                        if (item.Claimed > 0)
                        {
                            foreach (var task in OldGame.World.Tasks.ToList())
                            {
                                if (task.Claims.ContainsKey(item.EID))
                                {
                                    task.Unassign();
                                }
                            }
                        }
                    }
                    else
                    {
                        item.Owned = true;
                    }
                }
            }
        }
    }
}
