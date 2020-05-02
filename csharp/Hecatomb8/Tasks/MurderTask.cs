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
    class MurderTask : Task
    {
        public MurderTask() : base()
        {
            _name = "declare hostile";
            RequiresStructures = new List<Type>() { typeof(GuardPost) };
            _bg = "red";
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectTileControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            InterfaceState.SetControls(c);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
                return;
            }
            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr == null || cr == Player)
            {
                co.InfoMiddle = new List<ColoredText>() { "{yellow}Not a valid target." };
                return;
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + $"Declare hostility to {cr.Describe()}." };
                return;
            }


        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) || !HecatombOptions.Explored)
            {
                return false;
            }
            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr != null && cr != Player)
            {
                return true;
            }
            return false;
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "MurderTask", x: c.X, y: c.Y, z: c.Z);

            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr != null && cr != Player)
            {
                //Actor actor = cr.GetComponent<Actor>();
                //if (actor.Team == Teams.Friendly)
                //{
                //    actor.Team = Teams.Berserk;
                //}
                //else if (actor.Team == Teams.Neutral)
                //{
                //    // arguably all nearby neutrals should turn hostile?
                //    actor.Team = Teams.Hostile;
                //}
            }
        }
    }
}
