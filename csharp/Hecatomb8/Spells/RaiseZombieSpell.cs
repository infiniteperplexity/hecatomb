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
                else if (f is Grave && Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z - 1) is null && Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null)
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
                    var zombie = Entity.Spawn<Zombie>();
                    GetState<TaskHandler>().AddMinion(zombie);
                    zombie.PlaceInValidEmptyTile(c.X, c.Y, c.Z - 1);
                    if (!Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z - 1).Solid && (Explored.Contains(new Coord(c.X, c.Y, c.Z - 1)) || HecatombOptions.Explored))
                    {
                        (f as Grave)!.Shatter();
                        Senses.Announce(c.X, c.Y, c.Z, sight: "The zombie emerges into the tunnel below.");
                    }
                    else
                    {
                        Task emerge = Entity.Spawn<ZombieEmergeTask>();
                        emerge.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                        emerge.AssignTo(zombie);    
                    }
     
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
            MockupTaskName = "zombie emerging";
            _bg = "orange";
        }
        public override void Act()
        {
            Work();
        }

        public override bool ValidTile(Coord c)
        {
            Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (f is Grave)
            {
                return true;
            }
            return false;
        }
        public override void Start()
        {
            if (!Placed)
            {
                Unassign();
                return;
            }
            var (x, y, z) = GetVerifiedCoord();
            Senses.Announce(x, y, z, sight: "You hear an ominous stirring from under the ground...");
            // this code would place an incomplete fixture if there is no grave, but...Makes is null...I don't think that's safe
            //Feature? f = Features.GetWithBoundsChecked(x, y, z);
            //if (f is null)
            //{
            //    base.Start();
            //    f = Features.GetWithBoundsChecked(x, y, z);
                //if (f is IncompleteFixture)
                //{
                //    var ifx = (IncompleteFixture)f;
                //    ifx.IncompleteSymbol = '\u2717';
                //    ifx.IncompleteFG = "white";
                //    // it doesn't have a "makes"...
                //}
            //}
        }
        public override void Finish()
        {
            if (!Placed)
            {
                base.Finish();
            }
            var (X, Y, Z) = GetVerifiedCoord();
            Publish(new TutorialEvent() { Action = "ZombieEmerges" });
            Senses.Announce(X, Y, Z, sight: "A zombie bursts forth from the ground!");
            Feature? f = Features.GetWithBoundsChecked(X, Y, Z);
            if (f is Grave)
            {
                var grave = (Grave)f;
                grave.Shatter();
            }    
            else if (f != null)
            {
                f.Destroy();
            }
            Terrains.SetWithBoundsChecked(X, Y, Z, Terrain.DownSlopeTile);
            Terrains.SetWithBoundsChecked(X, Y, Z - 1, Terrain.UpSlopeTile);
            Cover.ClearGroundCover(X, Y, Z);
            Cover.ClearGroundCover(X, Y, Z - 1);
            base.Finish();
            //Game.World.ValidateOutdoors();
        }
    }
}
