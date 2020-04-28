using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Grave : Feature
    {
        public Grave()
        {
            _name = "grave";
            _fg = "WALLFG";
            _symbol = '\u271D';
            Components.Add(new Harvestable() { Yields = new Dictionary<Resource, float>() { [Resource.Corpse] = 1 } });
        }

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
