using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class GrowthHandler : StateHandler
    {
        public GrowthHandler()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public List<Feature> GetFlowersOfType(Resource r)
        {
            return Features.Where<Feature>((Feature f) => f is Flower && (f as Flower)!.Dye == r).ToList();
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent te = (TurnBeginEvent)ge;
            var world = GameState.World!;
            int grassTurns = 10;
            if (te.Turn % grassTurns == 0)
            {
                int grassChance = 5;
                for (int x = 1; x < world.Width - 1; x++)
                {
                    for (int y = 1; y < world.Height - 1; y++)
                    {
                        int z = world.GetBoundedGroundLevel(x, y);
                        if (world.Outdoors[x, y, z] == 2 && (Terrains.GetWithBoundsChecked(x, y, z) == Terrain.FloorTile || Terrains.GetWithBoundsChecked(x, y, z) == Terrain.UpSlopeTile) && Covers.GetWithBoundsChecked(x, y, z) == Cover.NoCover && Features.GetWithBoundsChecked(x, y, z) is null)
                        {
                            if (world.Random.Next(grassChance)==0)
                            {
                                Covers.SetWithBoundsChecked(x, y, z, Cover.Grass);
                            }
                        }
                    }
                }
            }
            int flowerTurns = 25;
            if (te.Turn % flowerTurns == 0)
            {
                var flowers = new Dictionary<Resource, List<Feature>>();
                foreach (var resource in Resource.Flowers)
                {
                    if (world.Random.Next(2) == 0)
                    {
                        var list = GetFlowersOfType(resource).OrderBy((Feature f) => world.Random.NextDouble()).ToList();
                        if (list.Count > 0 && list.Count <= 24)
                        {
                            Feature f = list[0];
                            var (X, Y, Z) = f.GetValidCoordinate();
                            Coord? c = Tiles.NearbyTile(X, Y, Z, max: 2, min: 1, valid: (x, y, z) =>
                            {
                                return (
                                    Features.GetWithBoundsChecked(x, y, z) is null
                                    && Terrains.GetWithBoundsChecked(x, y, z) == Terrain.FloorTile
                                );
                            });
                            if (c != null)
                            {
                                Coord cc = (Coord)c;
                                Feature f1 = Flower.Spawn(resource);
                                Debug.WriteLine($"placing a new flower at {cc.X} {cc.Y} {cc.Z}");
                                f1.PlaceInValidEmptyTile(cc.X, cc.Y, cc.Z);
                            }
                        }
                    }
                }
            }
            return ge;
        }
    }
}
