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
    using static HecatombAliases;

    public class GrowthHandler : StateHandler
    {
        public GrowthHandler()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public List<Feature> GetFlowersOfType(string s)
        {
            return Game.World.Features.Where<Feature>((Feature f) => f.TypeName=="Flower" && f.GetComponent<RandomPaletteComponent>().RandomPaletteType == s).ToList();
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent te = (TurnBeginEvent)ge;
            
            int grassTurns = 10;     
            if (te.Turn%grassTurns==0)
            {
                // grass
                int grassChance = 5;
                for (int x=1; x<Game.World.Width-1; x++)
                {
                    for (int y = 1; y < Game.World.Width - 1; y++)
                    {
                        int z = Game.World.GetGroundLevel(x, y);
                        if (Game.World.Outdoors[x, y, z]==2 && (Game.World.Terrains[x, y, z] == Terrain.FloorTile || Game.World.Terrains[x, y, z] == Terrain.UpSlopeTile) && Game.World.Covers[x, y, z] == Cover.NoCover && Game.World.Features[x, y, z]==null)
                        {
                            if (Game.World.Random.Arbitrary(grassChance, OwnSeed()) == 0)
                            //if (Game.World.Random.Next(chance)==0)
                            {
                                Game.World.Covers[x, y, z] = Cover.Grass;
                            }
                        }
                    }
                }
            }
            int flowerTurns = 25;
            if (te.Turn%flowerTurns==0)
            {
                var flowers = new Dictionary<string, List<Feature>>();
                foreach (var tuple in RandomPaletteHandler.FlowerNames)
                {
                    flowers[tuple.Item1] = new List<Feature>();
                }
                foreach (Feature f in Game.World.Features)
                {
                    if (f.TypeName == "Flower")
                    {
                        flowers[f.GetComponent<RandomPaletteComponent>().RandomPaletteType].Add(f);
                    }
                }
                foreach (var tuple in RandomPaletteHandler.FlowerNames)
                { 
                    if (Game.World.Random.Next(2)==0)
                    {
                        var list = flowers[tuple.Item1].OrderBy((Feature f) => Game.World.Random.Arbitrary(f.OwnSeed())).ToList();
                        if (list.Count > 0 && list.Count <= 24)
                        {
                            Feature f = list[0];
                            Coord? c = Tiles.NearbyTile(f.X, f.Y, f.Z, max: 2, min: 1, valid: (x, y, z) =>
                            {
                                return (
                                    Game.World.Features[x, y, z] == null
                                    && Game.World.Terrains[x, y, z] == Terrain.FloorTile
                                );
                            });
                            if (c != null)
                            {
                                Coord cc = (Coord)c;
                                Feature f1 = RandomPaletteHandler.SpawnFlower(tuple.Item1);
                                Debug.WriteLine($"placing a new flower at {cc.X} {cc.Y} {cc.Z}");
                                f1.Place(cc.X, cc.Y, cc.Z);
                            }
                        }
                    }   
                }
            }
            
            return ge;
        }
    }
}
