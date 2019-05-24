using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    class LairHandler : StateHandler
    {


        public bool FindHillSide()
        {
            // pick a random spot along the north wall for now
            int y = Game.World.Random.Next(25) + 12;
            int x = Game.World.Random.Next(100) + 75;
            int z = Game.World.GetGroundLevel(x, y);
            if (Game.World.Covers[x, y, z].Liquid)
            {
                return false;
            }
            int maxtries = 25;
            // then drive southward looking for a hillside
            for (int i = 0; i < maxtries; i++)
            {
                if (Game.World.Terrains[x, y + i, z].Solid)
                {
                    if (CheckBoundingBox(x, y + i, z))
                    {
                        BuildDwarfLair(x, y + i, z);
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }

        // always south for now
        public bool CheckBoundingBox(int x, int y, int z)
        {
            int hallwayLength = 3;
            for (int i = -6; i <= +6; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    // need to check world bounds too
                    if (!Game.World.Terrains[x + i, y + j + hallwayLength - 1, z].Solid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void BuildDwarfLair(int x, int y, int z)
        {
            Debug.WriteLine($"building a dwarf lair south of {x} {y} {z}");
            int hallwayLength = 3;
            for (int i = 0; i < hallwayLength; i++)
            {
                Game.World.Terrains[x, y + i, z] = Terrain.FloorTile;
                Game.World.Covers[x, y + i, z] = Cover.NoCover;
            }
            for (int i = -5; i <= +5; i++)
            {
                for (int j = 1; j < 12; j++)
                {
                    Game.World.Terrains[x + i, y + j + hallwayLength - 1, z] = Terrain.FloorTile;
                    Game.World.Covers[x + i, y + j + hallwayLength - 1, z] = Cover.NoCover;
                }
            }
            // smooth out the nearby slopes
            for (int i = -1; i <= +1; i++)
            {
                for (int j = -1; j <= +1; j++)
                {
                    if (Game.World.Terrains[x + i, y + j, z] == Terrain.UpSlopeTile)
                    {
                        Game.World.Terrains[x + i, y + j, z] = Terrain.FloorTile;
                        Game.World.Terrains[x + i, y + j, z + 1] = Terrain.EmptyTile;
                    }
                }
            }
        }


        public void PlaceDwarfLair()
        {
            int maxTries = 500;
            for (int i = 0; i < maxTries; i++)
            {
                if (FindHillSide())
                {
                    Debug.WriteLine($"placed a dwarf lair after {i + 1} tries");
                    return;
                }
            }
            Debug.WriteLine("failed to place a dwarf lair");
        }
    }
}
