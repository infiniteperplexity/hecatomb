using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Mausoleum : Feature
    {
        public Mausoleum()
        {
            _name = "mausoleum";
            _fg = "WALLFG";
            _symbol = '\u271F';
            Components.Add(new Harvestable() { Yields = new JsonArrayDictionary<Resource, float>() { [Resource.Gold] = 1, [Resource.Corpse] = 1 } });
            AddListener<StepEvent>(OnStep);
        }

        public GameEvent OnStep(GameEvent ge)
        {
            if (HecatombOptions.NoGhouls || !Spawned || !Placed)
            {
                return ge;
            }
            StepEvent se = (StepEvent)ge;
            Creature cr = se.Entity!;
            if (cr.GetComponent<Actor>().Team == Team.Friendly)
            {
                var world = GameState.World!;
                var (x, y, z) = GetPlacedCoordinate();
                if (z != world.GetBoundedGroundLevel(x, y))
                {
                    return ge;
                }
                if (Tiles.Distance(x, y, z, se.X, se.Y, se.Z) <= 4)
                {
                    EmergeGhoul();
                }
                else if (Tiles.Distance(x, y, z, se.X, se.Y, se.Z) <= 4 && world.Random.Next(3)==0)
                {
                    EmergeGhoul();
                }
            }
            return ge;
        }

        public void EmergeGhoul()
        {
            if (!Spawned || !Placed)
            {
                return;
            }
            var world = GameState.World!;
            var (x, y, z) = GetPlacedCoordinate();
            if (Creatures.GetWithBoundsChecked(x, y, z) != null)
            {
                return;
            }
            ParticleEmitter emitter = new ParticleEmitter();
            emitter.Place(x, y, z);
            Shatter();
            world.Terrains.SetWithBoundsChecked(x, y, z, Terrain.DownSlopeTile);
            world.Terrains.SetWithBoundsChecked(x, y, z - 1, Terrain.UpSlopeTile);
            Cover.ClearGroundCover(x, y, z);
            Cover.ClearGroundCover(x, y, z - 1);
            Ghoul ghoul = Entity.Spawn<Ghoul>();
            ghoul.PlaceInValidEmptyTile(x, y, z - 1);
            var task = Tasks.GetWithBoundsChecked(x, y, z);
            // in case we were trying to dig up the mausoleum or something
            task?.Cancel();
            ghoul.GetComponent<Inventory>().GrantItem(Item.SpawnNewResource(Resource.Gold, 1));
            Senses.Announce(x, y, z, sight: "{red}A ravenous ghoul bursts forth from its grave!");
        }

        // logic copied from grave
        public void Shatter()
        {
            int x = (int)X!;
            int y = (int)Y!;
            int z = (int)Z!;
            foreach (Coord c in Tiles.GetNeighbors8(x, y, z))
            {
                int x1 = c.X;
                int y1 = c.Y;
                int z1 = c.Z;
                if (Features.GetWithBoundsChecked(x1, y1, z1) is null && !Terrains.GetWithBoundsChecked(x1, y1, z1).Solid && !Terrains.GetWithBoundsChecked(x1, y1, z1).Fallable)
                {
                    if (GameState.World!.Random.Next(2) == 0)
                    {
                        var rock = Item.SpawnNewResource(Resource.Rock, 1);
                        rock.DropOnValidTile(x1, y1, z1);
                    }
                }
            }
            Destroy();
        }
    }
}
