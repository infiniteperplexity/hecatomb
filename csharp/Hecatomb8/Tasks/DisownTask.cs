using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class DisownTask : Task, ISelectsZone, IMenuListable
    {
        public DisownTask() : base()
        {
            _name = "disown or claim items";
            RequiresStructures = new List<Type>() { typeof(Stockpile) };
            _bg = "pink";
        }

        public override void ChooseFromMenu()
        {
            var c = new SelectZoneControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Toggle item claims." };
            InterfaceState.SetControls(c);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Toggle item claims from {0} {1} {2}", c.X, c.Y, c.Z) };
        }
        public override void TileHover(Coord c, List<Coord> squares)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Toggle item claims to {0} {1} {2}", c.X, c.Y, c.Z) };
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
                var item = Items.GetWithBoundsChecked(c.X, c.Y, c.Z);
                if (item != null)
                {
                    if (!item.Disowned)
                    {
                        item.Disowned = true;
                        // this will miss HaulTasks
                        if (item.Claimed > 0)
                        {
                            foreach (var task in Tasks.ToList())
                            {
                                if (task.Claims.ContainsKey((int)item.EID!))
                                {
                                    task.Unassign();
                                }
                            }
                        }
                    }
                    else
                    {
                        item.Disowned = false;
                    }
                }
            }
        }
    }
}
