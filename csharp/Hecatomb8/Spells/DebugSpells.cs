using System;
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
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                var c = new SelectTileControls(this);
                c.SelectedMenuCommand = "Spells";
                c.MenuSelectable = false;
                ControlContext.Set(c);
            }
        }

        public void SelectTile(Coord c)
        {
            Creature zombie = Entity.Spawn<Creature>("Zombie");
            zombie.GetComponent<Actor>().Team = Teams.Friendly;
            zombie.Place(c.X, c.Y, c.Z);
            int randomDecay = OldGame.World.Random.Next(500);
            zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
            zombie.GetComponent<Decaying>().Decay += randomDecay;
            GetState<TaskHandler>().Minions.Add(zombie);
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Feature f = OldGame.World.Features[x, y, z];
            if (!OldGame.World.Explored.Contains(c) && !Options.Explored)
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (f != null && f.TypeName == "Grave")
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            }
            else
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
            }
        }
    }



    public class DebugBanditSpell : Spell, ISelectsTile
    {
        public DebugBanditSpell() : base()
        {
            MenuName = "spawn bandit (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                ControlContext.Set(new SelectTileControls(this));
            }
        }

        public void SelectTile(Coord c)
        {
            Creature bandit = Entity.Spawn<Creature>("RagingDryad");
            bandit.Place(c.X, c.Y, c.Z);
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Feature f = OldGame.World.Features[x, y, z];
            if (!OldGame.World.Explored.Contains(c) && !Options.Explored)
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (f != null && f.TypeName == "Grave")
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            }
            else
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
            }
        }
    }

    public class DebugHealSpell : Spell
    {
        public DebugHealSpell()
        {
            MenuName = "self heal (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            base.ChooseFromMenu();
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                Cast();
            }
        }
        public override void Cast()
        {
            Caster.GetComponent<Defender>().Wounds = 0;
        }
    }

    public class DebugItemSpell : Spell, ISelectsTile
    {
        public DebugItemSpell() : base()
        {
            MenuName = "spawn item (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                ControlContext.Set(new SelectTileControls(this));
            }
        }

        public void SelectTile(Coord c)
        {
            //Item.SpawnCorpse().Place(c.X, c.Y, c.Z);
            string item = "Rock";
            if (OldGame.World.Random.Next(2) == 0)
            {
                item = "Wood";
            }
            Item.PlaceNewResource(item, 1, c.X, c.Y, c.Z);
        }

        public void TileHover(Coord c)
        {
        }
    }

    public class DebugFlowerSpell : Spell, ISelectsTile
    {
        public DebugFlowerSpell() : base()
        {
            MenuName = "spawn flower (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                ControlContext.Set(new SelectTileControls(this));
            }
        }

        static int NextFlower = 0;
        public void SelectTile(Coord c)
        {
            var handler = OldGame.World.GetState<RandomPaletteHandler>();
            var s = RandomPaletteHandler.FlowerNames[NextFlower].Item1;
            Feature f = RandomPaletteHandler.SpawnFlower(s);
            f.Place(c.X, c.Y, c.Z);  
            NextFlower = (NextFlower + 1) % RandomPaletteHandler.FlowerNames.Count;
        }

        public void TileHover(Coord c)
        {
        }
    }
}