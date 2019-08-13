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
            // pick a random spot nearish to the edge of the map
            int side = Game.World.Random.Next(4);
            int x, y;
            if (side == 0)
            {
                x = 25;
                y = Game.World.Random.Next(1, 254);
            }
            else if (side == 1)
            {
                x = 230;
                y = Game.World.Random.Next(1, 254);
            }
            else if (side == 2)
            {
                y = 25;
                x = Game.World.Random.Next(1, 254);
            }
            else
            {
                y = 230;
                x = Game.World.Random.Next(1, 254);
            }
            int z = Game.World.GetGroundLevel(x, y);
            if (Game.World.Covers[x, y, z].Liquid)
            {
                return false;
            }

            int dir = Game.World.Random.Next(4);
            int dx, dy;
            if (dir == 0)
            {
                dx = 1;
                dy = 0;
            }
            else if (dir == 1)
            {
                dx = -1;
                dy = 0;
            }
            else if (dir == 2)
            {
                dx = 0;
                dy = 1;
            }
            else
            {
                dx = 0;
                dy = -1;
            }

            int maxtries = 25;
            // then drive southward looking for a hillside
            for (int i = 0; i < maxtries; i++)
            {
                if (Game.World.Terrains[x + i * dx, y + i * dy, z].Solid)
                {
                    int hallwayLength = 3;
                    int setPieceRadius = 6;
                    int x0 = x + (i + hallwayLength + setPieceRadius - 1) * dx;
                    int y0 = y + (i + hallwayLength + setPieceRadius - 1) * dy;
                    if (CheckBoundingBox(x0, y0, z, dx, dy))
                    {
                        BuildDwarfLair(x0, y0, z, dx, dy);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool CheckBoundingBox(int x0, int y0, int z, int dx, int dy)
        {
            int setPieceRadius = 6;
            for (int i = -setPieceRadius; i <= +setPieceRadius ; i++)
            {
                for (int j = -setPieceRadius; j <= +setPieceRadius; j++)
                {
                    // out of bounds
                    if (x0 + i < 1 || x0 + i > Game.World.Width - 2 || y0 + j < 1 || y0 + j > Game.World.Width - 2)
                    {
                        return false;
                    }
                    // not solid rock
                    if (!Game.World.Terrains[x0 + i, y0 + j, z].Solid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void BuildDwarfLair(int x0, int y0, int z, int dx, int dy)
        {
            Debug.WriteLine($"building a dwarf lair centered on {x0} {y0} {z}");
            int hallwayLength = 3;
            int setPieceRadius = 6;
            // the top left corner
            int x1 = x0 - setPieceRadius;
            int y1 = y0 - setPieceRadius;
            for (int i = 0; i < hallwayLength; i++)
            {
                Game.World.Terrains[x0 - (setPieceRadius + i) * dx, y0 - (setPieceRadius + i) * dy, z] = Terrain.FloorTile;
                Game.World.Covers[x0 - (setPieceRadius + i) * dx, y0 - (setPieceRadius + i) * dy, z] = Cover.NoCover;
            }
            for (int i = 1; i < 12; i++)
            {
                for (int j = 1; j < 12; j++)
                {
                    // wall it off into rooms
                    if (i % 4 != 0 && j % 4 != 0)
                    {
                        Game.World.Terrains[x1 + i, y1 + j, z] = Terrain.FloorTile;
                        Game.World.Covers[x1 + i, y1 + j, z] = Cover.NoCover;
                    }
                }
            }
            // dig holes in the walls to connect rooms
            var maze = Maze.Generate(3, 3);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    bool right = maze[i, j, 0];
                    bool down = maze[i, j, 1];
                    if (i < 2 && !right)
                    {
                        Game.World.Terrains[x1 + (i + 1) * 4, y1 + (j + 1) * 4 - 2, z] = Terrain.FloorTile;
                        Game.World.Covers[x1 + (i + 1) * 4, y1 + (j + 1) * 4 - 2, z] = Cover.NoCover;
                    }
                    if (j < 2 && !down)
                    {
                        Game.World.Terrains[x1 + (i + 1) * 4 - 2, y1 + (j + 1) * 4, z] = Terrain.FloorTile;
                        Game.World.Covers[x1 + (i + 1) * 4 - 2, y1 + (j + 1) * 4, z] = Cover.NoCover;
                    }
                    if (Game.World.Random.Next(3) > 0)
                    {
                        var goblin = Entity.Spawn<Creature>("Goblin");
                        if (Game.World.Random.Next(3) == 0)
                        {
                            goblin.GetComponent<Inventory>().Item = Item.SpawnNewResource("TradeGoods", 1);
                        }
                        goblin.Place(x0 + 4*i - 4, y0 + 4*j - 4, z);
                    }
                }
            }

            // smooth out the nearby slopes
            for (int i = -1; i <= +1; i++)
            {
                for (int j = -1; j <= +1; j++)
                {
                    int x2 = x0 - (hallwayLength + setPieceRadius) * dx;
                    int y2 = y0 - (hallwayLength + setPieceRadius) * dy;
                    if (Game.World.Terrains[x2 + i, y2 + j, z] == Terrain.UpSlopeTile)
                    {
                        Game.World.Terrains[x2 + i, y2 + j, z] = Terrain.FloorTile;
                        Game.World.Terrains[x2 + i, y2 + j, z + 1] = Terrain.EmptyTile;
                    }
                }
            }
        }


        public void PlaceDwarfLair()
        {
            int maxTries = 1000;
            for (int i = 0; i < maxTries; i++)
            {
                if (FindHillSide())
                {
                    return;
                }
            }
        }
    }
}
