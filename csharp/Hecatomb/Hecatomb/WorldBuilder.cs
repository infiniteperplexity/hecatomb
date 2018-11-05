/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 3:02 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of WorldBuilder.
	/// </summary>
	public abstract class WorldBuilder
	{
		public virtual void Build(GameWorld world)
		{
			world.Reset();
		}
	}

	public class DefaultBuilder : WorldBuilder
	{
		public override void Build(GameWorld world)
		{
			base.Build(world);
			int GroundLevel = 50;
			float hscale = 2f;
			float vscale = 5f;
			var ElevationNoise = new FastNoise(seed: world.Random.Next(1024));
			var VegetationNoise = new FastNoise(seed: world.Random.Next(1024));
			for (int i=0; i<world.Width; i++) {
				for (int j=0; j<world.Height; j++) {
					for (int k=0; k<world.Depth; k++) {
						int elev = GroundLevel + (int) (vscale*ElevationNoise.GetSimplexFractal(hscale*i,hscale*j));
						if (i==0 || i==world.Width-1 || j==0 || j==world.Height-1 || k<elev) {
							world.Tiles[i,j,k] = Terrain.WallTile;
                            world.Covers[i,j,k] = Cover.Soil;
						} else if (k==elev) {
							world.Tiles[i,j,k] = Terrain.FloorTile;
                            if (k<=48)
                            {
                                world.Covers[i, j, k] = Cover.Water;
                            }
                            else
                            {
                                world.Covers[i, j, k] = Cover.Grass;
                            }
                        } else {
                            world.Tiles[i, j, k] = Terrain.EmptyTile;
                            if (k <= 48)
                            {
                                world.Covers[i, j, k] = Cover.Water;
                            }
                            else
                            {
                                world.Covers[i, j, k] = Cover.NoCover;
                            }
                        }
					}
				}
			}
			for (int i=1; i<world.Width-1; i++) {
				for (int j=1; j<world.Height-1; j++) {
					int k =world.GetGroundLevel(i, j);
					List<Coord> neighbors = Tiles.GetNeighbors8(i, j, k);
					bool slope = false;
					foreach (Coord c in neighbors)
					{
						if (world.GetTile(c.X, c.Y, c.Z) == Terrain.WallTile)
						{
							slope = true;
							break;
						}
					}

					if (slope)
					{
						world.Tiles[i, j, k] = Terrain.UpSlopeTile;
						if (world.GetTile(i, j, k+1)==Terrain.EmptyTile)
						{
							world.Tiles[i, j, k+1] = Terrain.DownSlopeTile;
						}
					} else {
						if (world.Random.Next(250)==1)
						{
							//Creature rat = Game.World.Entities.Spawn<Creature>("PackRat");
							//rat.Place(i, j, k);
						}
						float plants = vscale*VegetationNoise.GetSimplexFractal(hscale*i,hscale*j);
						if (plants>1.0f)
						{
							if (world.Random.Next(2)==1)
							{
								Feature tree;
								if (world.Random.Next(2)==1)
								{
									tree = world.Entities.Spawn<Feature>("ClubTree");
								}
								else
								{
									tree = world.Entities.Spawn<Feature>("SpadeTree");
								}
								tree.Place(i, j, k);
							}
						}
						else
						{
							if (world.Random.Next(50)==0)
							{
								Feature grave = world.Entities.Spawn<Feature>("Grave");
								grave.Place(i, j, k);
							}
						}
					}
				}
			}
			int z = world.GetGroundLevel(50, 50);
			//Creature ghoul = world.Entities.Spawn<Creature>("HungryGhoul");
			//ghoul.Place(50, 50, z);
		}
	}
	
	public class IntroBuilder : WorldBuilder
	{
		public override void Build(GameWorld world)
		{
			base.Build(world);
			for (int i=0; i<world.Width; i++) {
				for (int j=0; j<world.Height; j++) {
                    if (i <= 16 && i > 9 && j <= 16 && j > 9)
                    {
                        world.Tiles[i, j, 0] = Terrain.FloorTile;
                    }
                    else
                    {
                        world.Tiles[i, j, 0] = Terrain.WallTile;
                    }
				}
			}
            RecursiveBacktracker maze = new RecursiveBacktracker(12, 12);
            for (int i=0; i<world.Width/2; i++)
            {
                for (int j=0; j<world.Height/2; j++)
                {
                    TaskEntity t = Game.World.Entities.Spawn<TaskEntity>("DigTask");
                    t.Place(2 * i, 2 * j, 0);
                    if (!maze.BottomWalls[i,j])
                    {
                        t = Game.World.Entities.Spawn<TaskEntity>("DigTask");
                        t.Place(2 * i, 2 * j + 1, 0);
                    }
                    if (!maze.RightWalls[i,j])
                    {
                        t = Game.World.Entities.Spawn<TaskEntity>("DigTask");
                        t.Place(2 * i + 1, 2 * j, 0);
                    }
                }
            }

            //Game.World.Entities.Spawn<Creature>("Necromancer").Place(12, 12, 0);
            Game.World.Entities.Spawn<Creature>("Zombie").Place(12, 13, 0);
            Game.World.Entities.Spawn<Creature>("Zombie").Place(12, 11, 0);
            Game.World.Entities.Spawn<Creature>("Zombie").Place(11, 12, 0);
            Game.World.Entities.Spawn<Creature>("Zombie").Place(13, 12, 0);
        }
	}
}
