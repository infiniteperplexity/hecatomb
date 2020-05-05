using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    class ForbidTask : Task
    {
        public ForbidTask() : base()
        {
            _name = "forbid area";
            RequiresStructures = new List<Type>() { typeof(GuardPost) };
            _bg = "red";
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectZoneControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Forbid area." };
            InterfaceState.SetControls(c);
        }

        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "ForbidTask", squares: squares);
            base.SelectZone(squares);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Forbid tiles from {0} {1} {2}", c.X, c.Y, c.Z) };
        }
        public override void TileHover(Coord c, List<Coord> squares)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Forbid tiles to {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override bool CanAssign(Creature c)
        {
            return false;
        }
    }
}

