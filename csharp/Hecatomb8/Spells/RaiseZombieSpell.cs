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

    public class RaiseZombieSpell : Spell, ISelectsTile
    {
        public RaiseZombieSpell() : base()
        {
            MenuName = "raise zombie";
        }

        public override int GetCost()
        {
            var minions = GetState<TaskHandler>().Minions;
            if (minions.Count == 0)
            {
                return 10;
            }
            else if (minions.Count == 1)
            {
                return 15;
            }
            else if (minions.Count == 2)
            {
                return 20;
            }
            else if (minions.Count == 3)
            {
                return 25;
            }
            else
            {
                return 30;
            }
        }

        public override void ChooseFromMenu()
        {
            if (GetCost() > Component.Sanity)
            {
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "ChooseRaiseZombie" });
                var c = new SelectTileControls(this);
                c.MenuSelectable = false;
                c.SelectedMenuCommand = "Spells";
                ControlContext.Set(c);
            }
        }

        public void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "RaiseZombie", x: c.X, y: c.Y, z: c.Z);
            Feature f = OldGame.World.Features[c.X, c.Y, c.Z];
            if ((OldGame.World.Explored.Contains(c) || Options.Explored) && f != null && f.TypeName == "Grave")
            { 
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
                OldGame.World.Events.Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
                if (OldGame.World.GetState<TaskHandler>().Minions.Count >= 3)
                {
                    OldGame.World.Events.Publish(new AchievementEvent() { Action = "RaiseFourthZombie" });
                }
                Cast();
                ParticleEmitter emitter = new ParticleEmitter();
                emitter.Place(c.X, c.Y, c.Z);
                Creature zombie = Entity.Spawn<Creature>("Zombie");
                // some chance of non-human zombie?
                zombie.Species = "Human";
                zombie.GetComponent<Actor>().Team = Teams.Friendly;
                zombie.Place(c.X, c.Y, c.Z - 1);
                int randomDecay = OldGame.World.Random.Arbitrary(500, c.OwnSeed());
                //int randomDecay = Game.World.Random.Next(500);
                zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
                zombie.GetComponent<Decaying>().Decay += randomDecay;
                if (!OldGame.World.Terrains[c.X, c.Y, c.Z - 1].Solid && OldGame.World.Explored.Contains(new Coord(c.X, c.Y, c.Z - 1)))
                {
                    Status.PushMessage("The zombie burrows downward into the space below.");
                    BreakTombstone(f);
                }
                else
                {
                    Task emerge = Entity.Spawn<ZombieEmergeTask>();
                    emerge.AssignTo(zombie);
                    emerge.Place(c.X, c.Y, c.Z);
                }
                GetState<TaskHandler>().Minions.Add(zombie);
                return;
            }
            Item i = Items[c.X, c.Y, c.Z];
            if ((OldGame.World.Explored.Contains(c) || Options.Explored) && i != null && i.Resource=="Corpse")
            {   
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
                OldGame.World.Events.Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
                Cast();
                ParticleEmitter emitter = new ParticleEmitter();
                emitter.Place(c.X, c.Y, c.Z);
                Creature zombie = Entity.Spawn<Creature>("Zombie");
                zombie.Species = i.CorpseSpecies;
                zombie.GetComponent<Actor>().Team = Teams.Friendly;
                zombie.Place(c.X, c.Y, c.Z);
                int randomDecay = OldGame.World.Random.Arbitrary(500, c.OwnSeed());
                //int randomDecay = Game.World.Random.Next(500);
                zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
                // need to keep an eye on how this mapping works
                zombie.GetComponent<Decaying>().Decay = 10*i.Decay + randomDecay;
                i.Despawn();
                Status.PushMessage("The corpse stirs to obey your commands.");
                GetState<TaskHandler>().Minions.Add(zombie);
                
                return;
            }
            // some notification of failure?
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

        public static void BreakTombstone(Feature f)
        {
            var (x, y, z) = f;
            int seed = f.OwnSeed();
            f.Destroy();
            foreach (Coord c in Tiles.GetNeighbors8(x, y, z))
            {
                int x1 = c.X;
                int y1 = c.Y;
                int z1 = c.Z;
                f = OldGame.World.Features[x1, y1, z1];
                if (OldGame.World.Features[x1, y1, z1] == null && !OldGame.World.Terrains[x1, y1, z1].Solid && !OldGame.World.Terrains[x1, y1, z1].Fallable)
                {
                    if (OldGame.World.Random.Arbitrary(2, seed) == 0)
                    //if (Game.World.Random.Next(2) == 0)
                    {
                        Item.PlaceNewResource("Rock", 1, x1, y1, z1);
                    }
                }
            }
        }
    }

    public class ZombieEmergeTask : Task
    {
        public ZombieEmergeTask() : base()
        {
            Name = "zombie emerging";
            BG = "orange";
        }
        public override void Act()
        {
            Work();
        }

        public override bool ValidTile(Coord c)
        {
            Feature f = OldGame.World.Features[c];
            if (f == null)
            {
                return false;
            }
            if (f.TypeName != "Grave")
            {
                return false;
            }
            return true;
        }
        public override void Start()
        {
            OldGame.World.Events.Publish(new SensoryEvent() { X = X, Y = Y, Z = Z, Sight = "You hear an ominous stirring from under the ground..." });
            Feature f = OldGame.World.Features[X, Y, Z];
            if (f == null)
            {
                base.Start();
                f = OldGame.World.Features[X, Y, Z];
                f.Symbol = '\u2717';
                f.FG = "white";
            }
        }
        public override void Finish()
        {

            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ZombieEmerges" });
            OldGame.World.Events.Publish(new SensoryEvent() { Sight = "A zombie bursts forth from the ground!", X = X, Y = Y, Z = Z });
            Feature f = OldGame.World.Features[X, Y, Z];
            RaiseZombieSpell.BreakTombstone(f);    
            OldGame.World.Terrains[X, Y, Z] = Terrain.DownSlopeTile;
            OldGame.World.Terrains[X, Y, Z - 1] = Terrain.UpSlopeTile;
            Cover.ClearCover(X, Y, Z);
            Cover.ClearCover(X, Y, Z - 1);
            base.Finish();
            OldGame.World.ValidateOutdoors();
        } 
    }
  
}
