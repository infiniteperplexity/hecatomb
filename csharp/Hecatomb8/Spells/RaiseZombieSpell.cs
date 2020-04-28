using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class RaiseZombieSpell : Spell, ISelectsTile
    {
        public RaiseZombieSpell() : base()
        {
            MenuName = "raise zombie";
            _cost = 0;
        }

        public override void ChooseFromMenu()
        {
            if (Cost > Component!.Sanity)
            {
                Publish(new TutorialEvent() { Action = "Cancel" });
            }
            else
            {
                Publish(new TutorialEvent() { Action = "ChooseRaiseZombie" });
                var c = new SelectTileControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Spells";
                InterfaceState.SetControls(c);
            }
        }

        public void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "RaiseZombie", x: c.X, y: c.Y, z: c.Z);
            
            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (GameState.World!.Explored.Contains(c) || HecatombOptions.Explored)
            {
                Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
                Item? i = Items.GetWithBoundsChecked(c.X, c.Y, c.Z);

                if (i is Corpse && Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z) is null)
                {
                    Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
                    Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
                    if (GetState<TaskHandler>().Minions.Count >= 3)
                    {
                        Publish(new AchievementEvent() { Action = "RaiseFourthZombie" });
                    }
                    Cast();
                    ParticleEmitter emitter = new ParticleEmitter();
                    emitter.Place(c.X, c.Y, c.Z);
                    i.Despawn();
                    Senses.Announce(c.X, c.Y, c.Z, sight: "The zombie rises to obey your commands.");
                    var zombie = Entity.Spawn<Zombie>();
                    GetState<TaskHandler>().AddMinion(zombie);
                    zombie.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    InterfaceState.Commands!.Act();
                }
                else if (f is Grave && Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z - 1) is null)
                {
                    Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
                    Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
                    if (GetState<TaskHandler>().Minions.Count >= 3)
                    {
                        Publish(new AchievementEvent() { Action = "RaiseFourthZombie" });
                    }
                    Cast();
                    ParticleEmitter emitter = new ParticleEmitter();
                    emitter.Place(c.X, c.Y, c.Z);
                    (f as Grave)!.Shatter();
                    var zombie = Entity.Spawn<Zombie>();
                    if (!Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z - 1).Solid && (Explored.Contains(new Coord(c.X, c.Y, c.Z - 1)) || HecatombOptions.Explored))
                    {
                        Senses.Announce(c.X, c.Y, c.Z, sight: "The zombie emerges into the tunnel below.");
                    }
                    else
                    {
                        Task emerge = Entity.Spawn<ZombieEmergeTask>();
                        emerge.AssignTo(zombie);
                        emerge.Place(c.X, c.Y, c.Z);
                    }
                    GetState<TaskHandler>().AddMinion(zombie);
                    zombie.PlaceInValidEmptyTile(c.X, c.Y, c.Z - 1);
                    InterfaceState.Commands!.Act();
                }
            }
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            Item? i = Items.GetWithBoundsChecked(x, y, z);
            var controls = InterfaceState.Controls;
            // I need to look for a corpse as well
            if (!GameState.World!.Explored.Contains(c) && !HecatombOptions.Explored)
            {
                controls.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (f is Grave || i is Corpse)
            {
                controls.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            }
            else
            {
                controls.InfoMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
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
            Feature f = Game.World.Features[c];
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
            Game.World.Events.Publish(new SensoryEvent() { X = X, Y = Y, Z = Z, Sight = "You hear an ominous stirring from under the ground..." });
            Feature f = Game.World.Features[X, Y, Z];
            if (f == null)
            {
                base.Start();
                f = Game.World.Features[X, Y, Z];
                f.Symbol = '\u2717';
                f.FG = "white";
            }
        }
        public override void Finish()
        {

            Game.World.Events.Publish(new TutorialEvent() { Action = "ZombieEmerges" });
            Game.World.Events.Publish(new SensoryEvent() { Sight = "A zombie bursts forth from the ground!", X = X, Y = Y, Z = Z });
            Feature f = Game.World.Features[X, Y, Z];
            RaiseZombieSpell.BreakTombstone(f);
            Game.World.Terrains[X, Y, Z] = Terrain.DownSlopeTile;
            Game.World.Terrains[X, Y, Z - 1] = Terrain.UpSlopeTile;
            Cover.ClearCover(X, Y, Z);
            Cover.ClearCover(X, Y, Z - 1);
            base.Finish();
            Game.World.ValidateOutdoors();
        }
    }
}
