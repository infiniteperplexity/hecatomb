﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class DebugZombieSpell : Spell, ISelectsTile
    {
        public DebugZombieSpell() : base()
        {
            MenuName = "spawn zombie (debug)";
            cost = 0;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                Game.Controls.Set(new SelectTileControls(this));
            }
        }

        public void SelectTile(Coord c)
        {
            Creature zombie = Entity.Spawn<Creature>("Zombie");
            zombie.GetComponent<Actor>().Team = Teams.Friendly;
            zombie.Place(c.X, c.Y, c.Z);
            int randomDecay = Game.World.Random.Next(500);
            zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
            zombie.GetComponent<Decaying>().Decay += randomDecay;
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Feature f = Game.World.Features[x, y, z];
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (f != null && f.TypeName == "Grave")
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            }
            else
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
            }
        }
    }



    public class DebugBanditSpell : Spell, ISelectsTile
    {
        public DebugBanditSpell() : base()
        {
            MenuName = "spawn bandit (debug)";
            cost = 0;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                Game.Controls.Set(new SelectTileControls(this));
            }
        }

        public void SelectTile(Coord c)
        {
            Creature bandit = Entity.Spawn<Creature>("HumanBandit");
            bandit.Place(c.X, c.Y, c.Z);
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Feature f = Game.World.Features[x, y, z];
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (f != null && f.TypeName == "Grave")
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            }
            else
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
            }
        }
    }
}