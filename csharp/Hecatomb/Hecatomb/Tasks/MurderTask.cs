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
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
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
                co.MenuMiddle = new List<ColoredText>() { "{green}"+$"Declare hostility to {cr}." };
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
            
            Creature cr = Creatures[c];
            if (cr != null && cr != Player)
            {
                cr.GetComponent<Actor>().DeclaredEnemy = true;
            }
        }
    }
}
