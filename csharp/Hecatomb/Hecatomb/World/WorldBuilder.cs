﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 3:02 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of WorldBuilder.
	/// </summary>
	public abstract class WorldBuilder
	{
		public virtual void Build(World world)
		{
			world.Reset();
		}
	}

	public class DefaultBuilder : WorldBuilder
	{
		public override void Build(World world)
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
							world.Terrains[i,j,k] = Terrain.WallTile;
                            world.Covers[i,j,k] = Cover.Soil;
						} else if (k==elev) {
							world.Terrains[i,j,k] = Terrain.FloorTile;
                            if (k<=48)
                            {
                                world.Covers[i, j, k] = Cover.Water;
                            }
                            else
                            {
                                world.Covers[i, j, k] = Cover.Grass;
                            }
                        } else {
                            world.Terrains[i, j, k] = Terrain.EmptyTile;
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
            placeSoils();
            placeOres();
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
						world.Terrains[i, j, k] = Terrain.UpSlopeTile;
						if (world.GetTile(i, j, k+1)==Terrain.EmptyTile)
						{
							world.Terrains[i, j, k+1] = Terrain.DownSlopeTile;
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
                                if (world.Covers[i, j, k].Liquid)
                                {
                                    tree = Entity.Spawn<Feature>("Seaweed"); ;
                                }
								else if (world.Random.Next(2)==1)
								{
									tree = Entity.Spawn<Feature>("ClubTree");
								}
								else
								{
									tree = Entity.Spawn<Feature>("SpadeTree");
								}
								tree.Place(i, j, k);
							}
						}
						else
						{
                            Func<int, int, int, bool> downslopes = (int x, int y, int zz) => (world.GetTile(x, y, zz) == Terrain.DownSlopeTile);
							if (world.Random.Next(50)==0 && !world.Covers[i, j, k].Liquid && Tiles.GetNeighbors8(i, j, k, where: downslopes).Count==0)
							{
								Feature grave = Entity.Spawn<Feature>("Grave");
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


        protected void placeSoils()
        {
            int soil = 1;
            int limestone = 5;
            int basalt = 12;
            int granite = 12;
            int bedrock = 64;
            List<Cover> layers = new List<Cover>();
            for (int i=0; i<soil; i++)
            {
                layers.Add(Cover.Soil);
            }
            for (int i = 0; i < limestone; i++)
            {
                layers.Add(Cover.Limestone);
            }
            for (int i = 0; i < basalt; i++)
            {
                layers.Add(Cover.Basalt);
            }
            for (int i = 0; i < granite; i++)
            {
                layers.Add(Cover.Granite);
            }
            for (int i = 0; i < bedrock; i++)
            {
                layers.Add(Cover.Bedrock);
            }
            for (int x=1; x<Game.World.Width-1; x++)
            {
                for (int y=1; y<Game.World.Height-1; y++)
                {
                    int z = Game.World.GetGroundLevel(x, y)-1;
                    for (int i=0; z-i>0; i++)
                    {
                        Cover c = layers[i];
                        if (Game.World.Random.Next(20)==0)
                        {
                            if (i<soil)
                            {
                                c = Cover.Limestone;
                            }
                            else if (i<soil+limestone)
                            {
                                c = Cover.Basalt;
                            }
                            else if (i < soil + limestone + basalt)
                            {
                                c = Cover.Granite;
                            }
                            else
                            {
                                c = Cover.Bedrock;
                            }
                        }
                        Game.World.Covers[x, y, z - i] = c;
                    }
                    
                }
            }
            Game.World.ValidateOutdoors();
        }
        protected void placeOres()
        {
            placeOres(512);
        }
        protected void placeOres(int n)
        {
            int nveins = n;
            int segmax = 5;
            int segmin = 2;
            int seglen = 3;
            for (int z = 1; z < Game.World.Depth - 1; z++)
            {
                for (int i = 0; i < nveins; i++)
                {
                    int x0 = Game.World.Random.Next(1, Game.World.Width - 2);
                    int y0 = Game.World.Random.Next(1, Game.World.Height - 2);
                    Cover c = Game.World.Covers[x0, y0, z];
                    if (c.Solid)
                    {
                        Cover[] choices = new Cover[] { Cover.FlintCluster, Cover.FlintCluster, Cover.FlintCluster, Cover.CoalSeam, Cover.CoalSeam };
                        Cover choice = choices[Game.World.Random.Next(choices.Length)];
                        double displace = Game.World.Random.NextDouble() * 256;
                        double angle = Game.World.Random.NextDouble() * 2 * Math.PI;
                        int segs = Game.World.Random.Next(segmin, segmax);
                        for (int j = 0; j < segs; j++)
                        {
                            int x1 = x0 + (int)(Math.Cos(angle) * seglen);
                            int y1 = y0 + (int)(Math.Sin(angle) * seglen);
                            List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
                            foreach (var coord in line)
                            {
                                (int x, int y, int _) = coord;
                                if (x > 0 && x < Game.World.Width - 2 && y > 0 && y < Game.World.Height - 2)
                                {
                                    if (Game.World.Random.Next(3) > 0 && Game.World.Covers[x, y, z].Solid)
                                    {
                                        Game.World.Covers[x, y, z] = choice;
                                    }
                                }
                            }
                            x0 = x1;
                            y0 = y1;
                        }
                    }
                }
            }
        }
    }

    public class IntroBuilder : WorldBuilder
    {
        public override void Build(World world)
        {
            base.Build(world);
            for (int i = 0; i < world.Width; i++) {
                for (int j = 0; j < world.Height; j++) {
                    if (i <= 16 && i > 9 && j <= 16 && j > 9)
                    {
                        world.Terrains[i, j, 0] = Terrain.FloorTile;
                    }
                    else
                    {
                        world.Terrains[i, j, 0] = Terrain.WallTile;
                    }
                }
            }
            RecursiveBacktracker maze = new RecursiveBacktracker(12, 12);
            for (int i = 0; i < world.Width / 2; i++)
            {
                for (int j = 0; j < world.Height / 2; j++)
                {
                    Task t = Entity.Spawn<DigTask>();
                    t.Place(2 * i, 2 * j, 0);
                    if (!maze.BottomWalls[i, j])
                    {
                        t = Entity.Spawn<DigTask>();
                        t.Place(2 * i, 2 * j + 1, 0);
                    }
                    if (!maze.RightWalls[i, j])
                    {
                        t = Entity.Spawn<DigTask>();
                        t.Place(2 * i + 1, 2 * j, 0);
                    }
                }
            }
            //Game.World.Entities.Spawn<Creature>("Necromancer").Place(12, 12, 0);
            Entity.Spawn<Creature>("Zombie").Place(12, 13, 0);
            Entity.Spawn<Creature>("Zombie").Place(12, 11, 0);
            Entity.Spawn<Creature>("Zombie").Place(11, 12, 0);
            Entity.Spawn<Creature>("Zombie").Place(13, 12, 0);
        }
  
    }
}