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
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    for (int k = 0; k < world.Depth; k++)
                    {
                        int elev = GroundLevel + (int)(vscale * ElevationNoise.GetSimplexFractal(hscale * i, hscale * j));
                        if (i == 0 || i == world.Width - 1 || j == 0 || j == world.Height - 1)
                        {
                            world.Terrains[i, j, k] = Terrain.VoidTile;
                            world.Covers[i, j, k] = Cover.NoCover;
                        }
                        else if (k < elev)
                        {
                            world.Terrains[i, j, k] = Terrain.WallTile;
                            world.Covers[i, j, k] = Cover.Soil;
                        }
                        else if (k == elev)
                        {
                            world.Terrains[i, j, k] = Terrain.FloorTile;
                            if (k <= 48)
                            {
                                world.Covers[i, j, k] = Cover.Water;
                            }
                            else
                            {
                                world.Covers[i, j, k] = Cover.Grass;
                            }
                        }
                        else
                        {
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
            makeSlopes();
            placeSoils();
            placeOres();
            plantFlowers();
            placeGraves();
            plantTrees();
            if (!Game.Options.NoBatCaves)
            {
                //world.GetState<CaveVaultTracker>().PlaceBatCaves();
                world.GetState<VaultHandler>().PlaceBatCaves();
            }
            if (!Game.Options.NoLairs)
            {
                int nLairs = 3;
                for (int i = 0; i < nLairs; i++)
                {
                    world.GetState<LairHandler>().PlaceLair();
                }
            }
            if (!Game.Options.NoCaverns)
            {
                world.GetState<VaultHandler>().DigCaverns(25);
                world.GetState<VaultHandler>().DigCaverns(40);
            }
            placeSpiders();
            World.WorldSafeToDraw = true;
        }

        public void setElevations()
        {

        }
        public void placeSpiders()
        {
            var world = Game.World;
            for (int i = 1; i < world.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetGroundLevel(i, j);
                    if (!Game.Options.NoSpiders)
                    {
                        if (world.Random.Next(200) == 1)
                        {
                            var s = Entity.Spawn<Creature>("Spider");
                            s.GetCachedActor();
                            s.Place(i, j, k);
                        }
                    }
                }
            }
        }

        public void plantTrees()
        {
            float hscale = 2f;
            float vscale = 5f;
            var world = Game.World;
            var VegetationNoise = new FastNoise(seed: world.Random.Next(1024));
            for (int i = 1; i < world.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetGroundLevel(i, j);
                    float plants = vscale * VegetationNoise.GetSimplexFractal(hscale * i, hscale * j);
                    if (plants > 1.0f)
                    {
                        if (world.Random.Next(2) == 1 && world.Terrains[i, j, k] == Terrain.FloorTile && Game.World.Features[i, j, k] == null)
                        {
                            Feature tree;
                            if (world.Covers[i, j, k].Liquid)
                            {
                                tree = Entity.Spawn<Feature>("Seaweed"); ;
                            }
                            else if (world.Random.Next(2) == 1)
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
                }
            }
        }


        public void makeSlopes()
        {
            var world = Game.World;
            for (int i = 1; i < world.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetGroundLevel(i, j);
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
                        if (world.GetTile(i, j, k + 1) == Terrain.EmptyTile)
                        {
                            world.Terrains[i, j, k + 1] = Terrain.DownSlopeTile;
                        }
                    }
                }
            }
        }

        protected void plantFlowers()
        {
            // make sure the stupid thing does it here, just in case
            Game.World.GetState<RandomPaletteHandler>().PickFlowerColors();
            int vchunks = 3;
            int hchunks = 3;
            int vchunk = Game.World.Height / vchunks;
            int hchunk = Game.World.Width / hchunks;
            for (var i = 0; i < hchunks; i++)
            {
                for (var j = 0; j < vchunks; j++)
                {
                    var flower = RandomPaletteHandler.FlowerNames[i * 3 + j].Item1;
                    int rx = Game.World.Random.Next(hchunk);
                    int ry = Game.World.Random.Next(vchunk);
                    int x = i * hchunk + rx;
                    int y = j * vchunk + ry;
                    // could require that it be a bit farther from the player
                    Coord? cc = Feature.FindPlace(x, y, 0, max: 8, expand: 32);
                    if (cc != null)
                    {
                        Coord c = (Coord)cc;
                        Debug.WriteLine("planting " + flower + " at " + c.X + " " + c.Y);
                        var f = RandomPaletteHandler.SpawnFlower(flower);
                        f.Place(c.X, c.Y, c.Z);
                    }
                }
            }

        }
        protected void placeSoils()
        {
            int soil = 1;
            int limestone = 5;
            int basalt = 12;
            int granite = 12;
            int bedrock = 64;
            List<Cover> layers = new List<Cover>();
            for (int i = 0; i < soil; i++)
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
            for (int x = 1; x < Game.World.Width - 1; x++)
            {
                for (int y = 1; y < Game.World.Height - 1; y++)
                {
                    int z = Game.World.GetGroundLevel(x, y) - 1;
                    for (int i = 0; z - i > 0; i++)
                    {
                        Cover c = layers[i];
                        if (Game.World.Random.Next(20) == 0)
                        {
                            if (i < soil)
                            {
                                c = Cover.Limestone;
                            }
                            else if (i < soil + limestone)
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
                        if (c == Cover.Soil && Game.World.Random.Next(16) == 0)
                        {
                            c = Cover.FlintCluster;
                        }
                    }

                }
            }
            Game.World.ValidateOutdoors();
        }
        protected void oldPlaceOres()
        {
            //placeOres(512);
        }

        protected void placeOres()
        {
            int segmax = 5;
            int segmin = 2;
            int seglen = 3;
            int chunk = 16;
            int nper = 5;
            Dictionary<Cover, List<Cover>> ores = new Dictionary<Cover, List<Cover>>();
            ores[Cover.Soil] = new List<Cover>() { Cover.FlintCluster, Cover.CoalSeam, Cover.FlintCluster };
            ores[Cover.Limestone] = new List<Cover>() { Cover.CopperVein, Cover.TinVein, Cover.CoalSeam };
            ores[Cover.Basalt] = new List<Cover>() { Cover.IronVein, Cover.IronVein, Cover.SilverVein };
            ores[Cover.Granite] = new List<Cover>() { Cover.GoldVein, Cover.SilverVein, Cover.TitaniumVein, Cover.CobaltVein };
            ores[Cover.Bedrock] = new List<Cover>() { Cover.CobaltVein, Cover.TitaniumVein, Cover.AdamantVein, Cover.ThoriumVein };
            for (int z = 1; z < Game.World.Depth - 1; z++)
            {
                for (int x = 0; x < Game.World.Width; x += chunk)
                {
                    for (int y = 0; y < Game.World.Height; y += chunk)
                    {
                        for (int i = 0; i < nper; i++)
                        {
                            int x0 = x + Game.World.Random.Next(16);
                            int y0 = y + Game.World.Random.Next(16);
                            Cover c = Game.World.Covers[x0, y0, z];
                            if (ores.ContainsKey(c))
                            {
                                //int rj = Game.World.Random.Next(ores[c].Count);
                                int rj = i % ores[c].Count;
                                Cover choice = ores[c][rj];
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
                                        (int _x, int _y, int _) = coord;
                                        if (_x > 0 && _x < Game.World.Width - 2 && _y > 0 && _y < Game.World.Height - 2)
                                        {
                                            if (Game.World.Random.Next(3) > 0 && Game.World.Covers[_x, _y, z].Solid)
                                            {
                                                Game.World.Covers[_x, _y, z] = choice;
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
        }

        protected void placeGraves()
        {
            var world = Game.World;
            Func<int, int, int, bool> downslopes = (int x, int y, int zz) => (world.GetTile(x, y, zz) == Terrain.DownSlopeTile);
            int chunk = 16;
            int nper = 3;
            for (int x = 0; x < Game.World.Width; x += chunk)
            {
                for (int y = 0; y < Game.World.Height; y += chunk)
                {
                    
                    int tries = 0;
                    int ntries = 100;
                    // maybe place a mausoleum
                    if (world.Random.Next(3) == 0)
                    {
                        bool placed = false;
                        while (!placed && tries < ntries)
                        {
                            tries += 1;
                            int x0 = x + world.Random.Next(16);
                            int y0 = y + world.Random.Next(16);
                            if (x0 > 2 && y0 > 2 && x0 < world.Width - 3 && y0 < world.Width - 3)
                            {
                                // don't put mausoleums close to the center
                                if (Tiles.QuickDistance(x0, y0, 0, world.Width/2, world.Height/2, 0) < 25)
                                {
                                    break;
                                }
                                int z = world.GetGroundLevel(x0, y0);
                                if (world.Features[x0, y0, z] == null && !world.Covers[x0, y0, z].Liquid)
                                {
                                    bool valid = true;
                                    for (int i = x0-2; i<=x0+2; i++)
                                    {
                                        for (int j = y0-2; j <= y0+2; j++)
                                        {
                                            if (world.Terrains[i, j, z] != Terrain.FloorTile || world.Features[i, j, z] != null || world.Covers[i, j, z].Liquid)
                                            {
                                                valid = false;
                                            }
                                        }
                                    }
                                    if (valid)
                                    {
                                        Feature f;
                                        f = Entity.Spawn<Feature>("Mausoleum");
                                        f.Place(x0, y0, z);
                                        world.Terrains[x0 + 1, y0, z] = Terrain.UpSlopeTile;
                                        world.Terrains[x0 + 1, y0, z + 1] = Terrain.DownSlopeTile;
                                        world.Terrains[x0 - 1, y0, z] = Terrain.UpSlopeTile;
                                        world.Terrains[x0 - 1, y0, z + 1] = Terrain.DownSlopeTile;
                                        world.Terrains[x0, y0 + 1, z] = Terrain.UpSlopeTile;
                                        world.Terrains[x0, y0 + 1, z + 1] = Terrain.DownSlopeTile;
                                        world.Terrains[x0, y0 - 1 , z] = Terrain.UpSlopeTile;
                                        world.Terrains[x0, y0 - 1, z + 1] = Terrain.DownSlopeTile;
                                        placed = true;

                                    }
                                }
                            }
                            if (tries > ntries)
                            {
                                break;
                            }
                        }
                    }
                    int nplaced = 0;
                    tries = 0;
                    ntries = 1000;
                    // place multiple graves
                    while (nplaced < nper && tries < ntries)
                    {
                        tries += 1;
                        int x0 = x + world.Random.Next(16);
                        int y0 = y + world.Random.Next(16);
                        if (x0 > 0 && y0 > 0 && x0 < world.Width - 1 && y0 < world.Width - 1)
                        {
                            int z = world.GetGroundLevel(x0, y0);
                            if (world.Terrains[x0, y0, z] == Terrain.FloorTile && world.Features[x0, y0, z] == null && x0 % 2 == y0 % 2 && !world.Covers[x0, y0, z].Liquid && Tiles.GetNeighbors8(x0, y0, z, where: downslopes).Count == 0)
                            {
                                Feature grave = Entity.Spawn<Feature>("Grave");
                                grave.Place(x0, y0, z);
                                nplaced += 1;
                            }
                        }
                        if (tries > ntries)
                        {
                            break;
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
                    for (int k = 0; k < world.Depth; k++) { 
                        world.Terrains[i, j, k] = Terrain.FloorTile;
                    }
                }
            }
            string[] lines = System.IO.File.ReadAllLines(@"../Content/ASCII_icon.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                for (int j = 0; j<line.Length; j++)
                {
                    if (j%0 == 0 && line[j] != '.')
                    {
                        string typ = (line[j] == '@') ? "IntroNecromancer" : "IntroZombie";

                        Creature cr = Entity.Spawn<Creature>(typ);
                        cr.Place(1 + j / 2, 1 + i / 2, 1);
                    }
                }
            }
            Game.Camera.Center(16, 16, 1);
            // need to handle exploration and visibility
            // need to handle special AI for intro drones, that sleeps to start and randomly wakes up
            // hide the player on another level maybe?
            // or just have no time pass...even maybe just have black backgrounds...make this not a world.
        }
    }
}
