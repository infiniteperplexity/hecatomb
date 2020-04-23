using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class RaiseZombieSpell : Spell, ISelectsTile
    {
        public RaiseZombieSpell() : base()
        {
            MenuName = "raise zombie";
        }

        public override void ChooseFromMenu()
        {
            if (Cost > Component!.Sanity)
            {
                //Game.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
                //Debug.WriteLine("cannot cast spell");
            }
            else
            {
                //Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseRaiseZombie" });
                var c = new SelectTileControls(this);
                //c.MenuSelectable = false;
                //c.SelectedMenuCommand = "Spells";
                InterfaceState.SetControls(c);
            }
        }

        public void SelectTile(Coord c)
        {
            if ((GameState.World!.Explored.Contains(c) || HecatombOptions.Explored) && Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z) is null)
            {
                Cast();
                //    ParticleEmitter emitter = new ParticleEmitter();
                //    emitter.Place(c.X, c.Y, c.Z);
                var zombie = Entity.Spawn<Zombie>();
                zombie.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                InterfaceState.Commands!.Act();
            }
        }




            //CommandLogger.LogCommand(command: "RaiseZombie", x: c.X, y: c.Y, z: c.Z);
            //Feature f = Game.World.Features[c.X, c.Y, c.Z];
            //if ((Game.World.Explored.Contains(c) || Options.Explored) && f != null && f.TypeName == "Grave")
            //{
            //    Game.World.Events.Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
            //    Game.World.Events.Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
            //    if (Game.World.GetState<TaskHandler>().Minions.Count >= 3)
            //    {
            //        Game.World.Events.Publish(new AchievementEvent() { Action = "RaiseFourthZombie" });
            //    }
            //    Cast();
            //    ParticleEmitter emitter = new ParticleEmitter();
            //    emitter.Place(c.X, c.Y, c.Z);
            //    Creature zombie = Entity.Spawn<Creature>("Zombie");
            //    // some chance of non-human zombie?
            //    zombie.Species = "Human";
            //    zombie.GetComponent<Actor>().Team = Teams.Friendly;
            //    zombie.Place(c.X, c.Y, c.Z - 1);
            //    int randomDecay = Game.World.Random.Arbitrary(500, c.OwnSeed());
            //    //int randomDecay = Game.World.Random.Next(500);
            //    zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
            //    zombie.GetComponent<Decaying>().Decay += randomDecay;
            //    if (!Game.World.Terrains[c.X, c.Y, c.Z - 1].Solid && Game.World.Explored.Contains(new Coord(c.X, c.Y, c.Z - 1)))
            //    {
            //        Status.PushMessage("The zombie burrows downward into the space below.");
            //        BreakTombstone(f);
            //    }
            //    else
            //    {
            //        Task emerge = Entity.Spawn<ZombieEmergeTask>();
            //        emerge.AssignTo(zombie);
            //        emerge.Place(c.X, c.Y, c.Z);
            //    }
            //    GetState<TaskHandler>().Minions.Add(zombie);
            //    return;
            //}
            //Item i = Items[c.X, c.Y, c.Z];
            //if ((Game.World.Explored.Contains(c) || Options.Explored) && i != null && i.Resource == "Corpse")
            //{
            //    Game.World.Events.Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
            //    Game.World.Events.Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
            //    Cast();
            //    ParticleEmitter emitter = new ParticleEmitter();
            //    emitter.Place(c.X, c.Y, c.Z);
            //    Creature zombie = Entity.Spawn<Creature>("Zombie");
            //    zombie.Species = i.CorpseSpecies;
            //    zombie.GetComponent<Actor>().Team = Teams.Friendly;
            //    zombie.Place(c.X, c.Y, c.Z);
            //    int randomDecay = Game.World.Random.Arbitrary(500, c.OwnSeed());
            //    //int randomDecay = Game.World.Random.Next(500);
            //    zombie.GetComponent<Decaying>().TotalDecay += randomDecay;
            //    // need to keep an eye on how this mapping works
            //    zombie.GetComponent<Decaying>().Decay = 10 * i.Decay + randomDecay;
            //    i.Despawn();
            //    Status.PushMessage("The corpse stirs to obey your commands.");
            //    GetState<TaskHandler>().Minions.Add(zombie);

            //    return;
            //}
            // some notification of failure?

        public void TileHover(Coord c)
        {
            //int x = c.X;
            //int y = c.Y;
            //int z = c.Z;
            //Feature f = Game.World.Features[x, y, z];
            //if (!Game.World.Explored.Contains(c) && !Options.Explored)
            //{
            //    Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            //}
            //else if (f != null && f.TypeName == "Grave")
            //{
            //    Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z) };
            //}
            //else
            //{
            //    Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Select a tile with a tombstone or corpse." };
            //}
        }

        //public static void BreakTombstone(Feature f)
        //{
        //    var (x, y, z) = f;
        //    int seed = f.OwnSeed();
        //    f.Destroy();
        //    foreach (Coord c in Tiles.GetNeighbors8(x, y, z))
        //    {
        //        int x1 = c.X;
        //        int y1 = c.Y;
        //        int z1 = c.Z;
        //        f = Game.World.Features[x1, y1, z1];
        //        if (Game.World.Features[x1, y1, z1] == null && !Game.World.Terrains[x1, y1, z1].Solid && !Game.World.Terrains[x1, y1, z1].Fallable)
        //        {
        //            if (Game.World.Random.Arbitrary(2, seed) == 0)
        //            //if (Game.World.Random.Next(2) == 0)
        //            {
        //                Item.PlaceNewResource("Rock", 1, x1, y1, z1);
        //            }
        //        }
        //    }
        //}
    }
}
