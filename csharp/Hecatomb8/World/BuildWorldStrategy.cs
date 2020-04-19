using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    class BuildWorldStrategy
    {
        public void Generate()
        {
            var world = GameState.World!;
            setElevations();
            makeSlopes();
            world.Player = Entity.Spawn<Necromancer>();
            var (x, y, z) = (1, 1, world.GetBoundedGroundLevel(1, 1));
            if (!world.Creatures.ContainsKey(x, y, z))
            {
                world.Player.PlaceInValidEmptyTile(x, y, z);
            }
            else
            {
                throw new Exception("Player can't be placed.");
            }
        }

        void setElevations()
        {
            var world = GameState.World!;
            int GroundLevel = 50;
            float hscale = 2f;
            float vscale = 5f;
            var ElevationNoise = new FastNoise(seed: world.Random.Next(1024));
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    for (int k = 0; k < world.Depth; k++)
                    {
                        int elev = GroundLevel + (int)(vscale * ElevationNoise.GetSimplexFractal(hscale * i, hscale * j));
                        if (i == 0 || i == world.Width - 1 || j == 0 || j == world.Height - 1)
                        {
                            world.Terrains.SetWithBoundsChecked(i, j, k, Terrain.VoidTile);
                            world.Covers.SetWithBoundsChecked(i, j, k, Cover.NoCover);
                        }
                        else if (k < elev)
                        {
                            world.Terrains.SetWithBoundsChecked(i, j, k, Terrain.WallTile);
                            world.Covers.SetWithBoundsChecked(i, j, k, Cover.Soil);
                        }
                        else if (k == elev)
                        {
                            world.Terrains.SetWithBoundsChecked(i, j, k, Terrain.FloorTile);
                            if (k <= 48)
                            {
                                world.Covers.SetWithBoundsChecked(i, j, k, Cover.Water);
                            }
                            else
                            {
                                world.Covers.SetWithBoundsChecked(i, j, k, Cover.Grass);
                            }
                        }
                        else
                        {
                            world.Terrains.SetWithBoundsChecked(i, j, k, Terrain.EmptyTile);
                            if (k <= 48)
                            {
                                world.Covers.SetWithBoundsChecked(i, j, k, Cover.Water);
                            }
                            else
                            {
                                world.Covers.SetWithBoundsChecked(i, j, k, Cover.NoCover);
                            }
                        }
                    }
                }
            }
        }
        
        public void makeSlopes()
        {
            var world = GameState.World;
            for (int i = 1; i < world!.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetBoundedGroundLevel(i, j);
                    List<Coord> neighbors = Tiles.GetNeighbors8(i, j, k);
                    bool slope = false;
                    foreach (Coord c in neighbors)
                    {
                        if (world.Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z) == Terrain.WallTile)
                        {
                            
                            slope = true;
                            break;
                        }
                    }
                    if (slope)
                    {
                        world.Terrains.SetWithBoundsChecked(i, j, k, Terrain.UpSlopeTile);
                        if (world.Terrains.GetWithBoundsChecked(i, j, k + 1) == Terrain.EmptyTile)
                        {
                            
                            world.Terrains.SetWithBoundsChecked(i, j, k + 1, Terrain.DownSlopeTile);
                        }
                    }
                }
            }
        }


    } 
}
