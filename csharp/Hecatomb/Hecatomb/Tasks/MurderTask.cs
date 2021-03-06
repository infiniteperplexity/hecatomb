﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    using static HecatombAliases;
    class MurderTask : Task
    {
        public MurderTask() : base()
        {
            MenuName = "declare hostile";
            // should a guard post be required to declare things hostile?
            //PrereqStructures = new List<string>() { "GuardPost" };
            BG = "red";
        }

        public override void ChooseFromMenu()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            var c = new SelectTileControls(this);
            c.MenuSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            ControlContext.Set(c);
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (!Explored.Contains(c) && !Options.Explored)
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
                return;
            }
            Creature cr = Creatures[c];
            if (cr==null || cr==Player)
            {
                co.MenuMiddle = new List<ColoredText>() { "{yellow}Not a valid target." };
                return;
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() { "{green}"+$"Declare hostility to {cr.Describe()}." };
                return;
            }
            
            
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) || !Options.Explored)
            {
                return false;
            }
            Creature cr = Creatures[c];
            if (cr != null && cr != Player)
            {
                return true;
            }
            return false;
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "MurderTask", x: c.X, y: c.Y, z: c.Z);

            Creature cr = Creatures[c];
            if (cr != null && cr != Player)
            {
                Actor actor = cr.GetComponent<Actor>();
                if (actor.Team == Teams.Friendly)
                {
                    actor.Team = Teams.Berserk;
                }
                else if (actor.Team == Teams.Neutral)
                {
                    // arguably all nearby neutrals should turn hostile?
                    actor.Team = Teams.Hostile;
                }
            }
        }
    }
}
