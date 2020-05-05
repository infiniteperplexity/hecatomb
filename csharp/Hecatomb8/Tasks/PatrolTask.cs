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
    class PatrolTask : Task
    {
        public PatrolTask() : base()
        {
            _name = "patrol area";
            RequiresStructures = new List<Type>() { typeof(GuardPost) };
            _bg = "#009999";
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectTileControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Patrol area."};
            InterfaceState.SetControls(c);
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "PatrolTask", x: c.X, y: c.Y, z: c.Z);
            base.SelectTile(c);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (ValidTile(c))
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Patrol area around {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
        }

        public override bool ValidTile(Coord c)
        {
            return true;
        }
        public override void Act()
        {
            if (!Spawned || !Placed || Worker?.UnboxBriefly() is null)
            {
                return;
            }
            Actor a = Worker.UnboxBriefly()!.GetComponent<Actor>();
            a.Patrol((int)X!, (int)Y!, (int)Z!);
        }
    }
}
