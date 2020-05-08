using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    class BuildWorldStrategy
    {
        public bool Generate()
        {
            var world = GameState.World!;
            world.SpawnHandlers();
            setElevations();
            makeSlopes();
            placeSoils();
            plantFlowers();
            placeOres();
            placeGraves();
            plantTrees();
            world.ValidateOutdoors();
            bool succeeded = tryToPlacePlayer();
            placeSpiders();
            return succeeded;           
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

        protected void plantFlowers()
        {
            var world = GameState.World!;
            int vchunks = 3;
            int hchunks = 3;
            int vchunk = world.Height / vchunks;
            int hchunk = world.Width / hchunks;
            for (var i = 0; i < hchunks; i++)
            {
                for (var j = 0; j < vchunks; j++)
                {
                    var list = Resource.Flowers.ToList();
                    list = list.OrderBy(f => world.Random.NextDouble()).ToList();
                    var flower = list[i * 3 + j];
                    int rx = world.Random.Next(hchunk);
                    int ry = world.Random.Next(vchunk);
                    int x = i * hchunk + rx;
                    int y = j * vchunk + ry;
                    // could require that it be a bit farther from the player
                    Coord? cc = Feature.FindPlace(x, y, 0, max: 8, expand: 32);
                    if (cc != null)
                    {
                        Coord c = (Coord)cc;
                        Debug.WriteLine("planting " + flower + " at " + c.X + " " + c.Y);
                        var f = Flower.Spawn(flower);
                        f.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    }
                }
            }

        }

        protected void placeSoils()
        {
            var world = GameState.World!;
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
            for (int x = 1; x < world.Width - 1; x++)
            {
                for (int y = 1; y < world.Height - 1; y++)
                {
                    int z = world.GetBoundedGroundLevel(x, y) - 1;
                    for (int i = 0; z - i > 0; i++)
                    {
                        Cover c = layers[i];
                        if (world.Random.Next(20) == 0)
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
                        Covers.SetWithBoundsChecked(x, y, z - i, c);
                        
                    }

                }
            }
            //Game.World.ValidateOutdoors();
        }

        protected void placeOres()
        {
            var world = GameState.World!;
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
            for (int z = 1; z < world.Depth - 1; z++)
            {
                for (int x = 0; x <world.Width; x += chunk)
                {
                    for (int y = 0; y < world.Height; y += chunk)
                    {
                        for (int i = 0; i < nper; i++)
                        {
                            int x0 = x + world.Random.Next(16);
                            int y0 = y + world.Random.Next(16);
                            Cover c = world.Covers.GetWithBoundsChecked(x0, y0, z);
                            if (ores.ContainsKey(c))
                            {
                                //int rj = Game.World.Random.Next(ores[c].Count);
                                int rj = i % ores[c].Count;
                                Cover choice = ores[c][rj];
                                double displace = world.Random.NextDouble() * 256;
                                double angle = world.Random.NextDouble() * 2 * Math.PI;
                                int segs = world.Random.Next(segmin, segmax);
                                for (int j = 0; j < segs; j++)
                                {
                                    int x1 = x0 + (int)(Math.Cos(angle) * seglen);
                                    int y1 = y0 + (int)(Math.Sin(angle) * seglen);
                                    List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
                                    foreach (var coord in line)
                                    {
                                        (int _x, int _y, int _) = coord;
                                        if (_x > 0 && _x < world.Width - 2 && _y > 0 && _y < world.Height - 2)
                                        {
                                            if (world.Random.Next(3) > 0 && Covers.GetWithBoundsChecked(_x, _y, z).Solid)
                                            {
                                                Covers.SetWithBoundsChecked(_x, _y, z, choice);
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

        public void plantTrees()
        {
            float hscale = 2f;
            float vscale = 5f;
            var world = GameState.World!;
            var VegetationNoise = new FastNoise(seed: world.Random.Next(1024));
            for (int i = 1; i < world.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetBoundedGroundLevel(i, j);
                    float plants = vscale * VegetationNoise.GetSimplexFractal(hscale * i, hscale * j);
                    if (plants > 1.0f)
                    {
                        if (world.Random.Next(2) == 1 && Terrains.GetWithBoundsChecked(i, j, k) == Terrain.FloorTile && Features.GetWithBoundsChecked(i, j, k) == null)
                        {
                            Feature? tree = Features.GetWithBoundsChecked(i, j, k);
                            if (tree is null)
                            {
                                if (world.Covers.GetWithBoundsChecked(i, j, k).Liquid)
                                {
                                    //tree = Entity.Spawn<SeaWeed>("Seaweed"); ;
                                }
                                else if (world.Random.Next(2) == 1)
                                {
                                    tree = Entity.Spawn<ClubTree>();
                                }
                                else
                                {
                                    tree = Entity.Spawn<SpadeTree>();
                                }
                                tree?.PlaceInValidEmptyTile(i, j, k);
                            }
                        }
                    }
                }
            }
        }

        protected void placeGraves()
        {
            var world = GameState.World!;
            Func<int, int, int, bool> downslopes = (int x, int y, int zz) => (Terrains.GetWithBoundsChecked(x, y, zz) == Terrain.DownSlopeTile);
            int chunk = 16;
            int nper = 3;
            for (int x = 0; x < world.Width; x += chunk)
            {
                for (int y = 0; y < world.Height; y += chunk)
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
                                if (Tiles.Distance(x0, y0, 0, world.Width / 2, world.Height / 2, 0) < 30)
                                {
                                    break;
                                }
                                int z = world.GetBoundedGroundLevel(x0, y0);
                                if (Features.GetWithBoundsChecked(x0, y0, z) is null && !Covers.GetWithBoundsChecked(x0, y0, z).Liquid)
                                {
                                    bool valid = true;
                                    for (int i = x0 - 2; i <= x0 + 2; i++)
                                    {
                                        for (int j = y0 - 2; j <= y0 + 2; j++)
                                        {
                                            if (Terrains.GetWithBoundsChecked(i, j, z) != Terrain.FloorTile || Features.GetWithBoundsChecked(i, j, z) != null || Covers.GetWithBoundsChecked(i, j, z).Liquid)
                                            {
                                                valid = false;
                                            }
                                        }
                                    }
                                    if (valid)
                                    {
                                        Feature f;
                                        f = Entity.Spawn<Mausoleum>();
                                        f.PlaceInValidEmptyTile(x0, y0, z);
                                        world.Terrains.SetWithBoundsChecked(x0 + 1, y0, z, Terrain.UpSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0 + 1, y0, z + 1, Terrain.DownSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0 - 1, y0, z, Terrain.UpSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0 - 1, y0, z + 1, Terrain.DownSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0, y0 + 1, z, Terrain.UpSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0, y0 + 1, z + 1, Terrain.DownSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0, y0 - 1, z, Terrain.UpSlopeTile);
                                        world.Terrains.SetWithBoundsChecked(x0, y0 - 1, z + 1, Terrain.DownSlopeTile);
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
                            int z = world.GetBoundedGroundLevel(x0, y0);
                            if (Terrains.GetWithBoundsChecked(x0, y0, z) == Terrain.FloorTile && Features.GetWithBoundsChecked(x0, y0, z) == null && x0 % 2 == y0 % 2 && !Covers.GetWithBoundsChecked(x0, y0, z).Liquid && Tiles.GetNeighbors8(x0, y0, z, where: downslopes).Count == 0)
                            {
                                Feature grave = Entity.Spawn<Grave>();
                                grave.PlaceInValidEmptyTile(x0, y0, z);
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

        public bool tryToPlacePlayer()
        {
            World world = GameState.World!;
            bool valid = true;
            var x = world.Width / 2;
            var y = world.Height / 2;
            var z = world.GetBoundedGroundLevel(world.Width / 2, world.Height / 2);
            if (Covers.GetWithBoundsChecked(x, y, z).Liquid)
            {
                valid = false;
            }
            var nearbyGraves = 0;
            var graves = 0;
            var trees = 0;
            foreach (Feature f in Features)
            {
                if (f is Grave && Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 15)
                {
                    graves += 1;
                    if (Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 5 && z == f.Z)
                    {
                        nearbyGraves += 1;
                    }
                }
                else if ((f is ClubTree || f is SpadeTree) && Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 15)
                {
                    trees += 1;
                }
            }
            if (nearbyGraves < 1 || graves < 3 || trees < 5)
            {
                valid = false;
            }
            var failures = 0;
            while (!valid)
            {
                failures += 1;
                if (failures >= 250)
                {
                    Debug.WriteLine("had trouble placing player, regenerating world.");
                    return false;
                }
                x += world.Random.Next(-5, 5);
                y += world.Random.Next(-5, 5);
                z = world.GetBoundedGroundLevel(x, y);
                valid = true;
                if (Covers.GetWithBoundsChecked(x, y, z).Liquid)
                {
                    valid = false;
                }
                nearbyGraves = 0;
                graves = 0;
                foreach (Feature f in Features)
                {
                    if (f is Grave && Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 15)
                    {
                        graves += 1;
                        if (Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 5 && z == f.Z)
                        {
                            nearbyGraves += 1;
                        }
                    }
                    else if ((f is ClubTree || f is SpadeTree) && Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) <= 15)
                    {
                        trees += 1;
                    }
                }
                if (nearbyGraves < 1 || graves < 3 || trees < 5)
                {
                    valid = false;
                }
            }
            int surfaceFlint = 0;
            int surfaceCoal = 0;
            for (int i = -15; i <= 15; i++)
            {
                for (int j = -15; j <= 15; j++)
                {
                    int k = world.GetBoundedGroundLevel(x + i, y + j);
                    if (Covers.GetWithBoundsChecked(x + i, y + j, k - 1) == Cover.FlintCluster)
                    {
                        surfaceFlint += 1;
                    }
                    else if (Covers.GetWithBoundsChecked(x + i, y + j, k - 1) == Cover.CoalSeam)
                    {
                        surfaceCoal += 1;
                    }
                }
            }
            Debug.WriteLine("nearby surface flint: " + surfaceFlint);
            Debug.WriteLine("nearby surface coal: " + surfaceCoal);
            while (surfaceFlint < 15)
            {
                int rx = x - 15 + world.Random.Next(31);
                int ry = y - 15 + world.Random.Next(31);
                if (Covers.GetWithBoundsChecked(rx, ry, world.GetBoundedGroundLevel(rx, ry) - 1) == Cover.Soil)
                {
                    Covers.SetWithBoundsChecked(rx, ry, world.GetBoundedGroundLevel(rx, ry) - 1, Cover.FlintCluster);
                    surfaceFlint += 1;
                }
            }
            while (surfaceCoal < 15)
            {
                int rx = x - 15 + world.Random.Next(31);
                int ry = y - 15 + world.Random.Next(31);
                if (Covers.GetWithBoundsChecked(rx, ry, world.GetBoundedGroundLevel(rx, ry) - 1) == Cover.Soil)
                {
                    Covers.SetWithBoundsChecked(rx, ry, world.GetBoundedGroundLevel(rx, ry) - 1, Cover.CoalSeam);
                    surfaceCoal += 1;
                }
            }
            Creatures.GetWithBoundsChecked(x, y, z)?.Despawn();
            Creature p = Entity.Spawn<Necromancer>();
            world.Player = p;
            //p.GetComponent<Actor>().Team = Teams.Friendly;
            p.PlaceInValidEmptyTile(x, y, z);
            //*****
            //foreach (var res in Game.Options.FreeStuff)
            //{
            //    Item.PlaceNewResource(res.Item1, res.Item2, x, y, z);
            //}
            return true;
        }

        public void placeSpiders()
        {
            if (HecatombOptions.NoSpiders)
            {
                return;
            }
            var world = GameState.World!;
            for (int i = 1; i < world.Width - 1; i++)
            {
                for (int j = 1; j < world.Height - 1; j++)
                {
                    int k = world.GetBoundedGroundLevel(i, j);
                    if (world.Random.Next(200) == 1)
                    {
                        if (Creatures.GetWithBoundsChecked(i, j, k) is null)
                        { 
                            var s = Entity.Spawn<Spider>();
                            s.PlaceInValidEmptyTile(i, j, k);
                        }
                    }
                }
            }
        }
    }
}
