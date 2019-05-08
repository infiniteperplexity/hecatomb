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

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent te = (TurnBeginEvent)ge;
            int nTurns = 10;
            int chance = 5;
            if (te.Turn%nTurns==0)
            {
                for (int x=1; x<Game.World.Width-1; x++)
                {
                    for (int y = 1; y < Game.World.Width - 1; y++)
                    {
                        int z = Game.World.GetGroundLevel(x, y);
                        if (Game.World.Outdoors[x, y, z]==2 && (Game.World.Terrains[x, y, z] == Terrain.FloorTile || Game.World.Terrains[x, y, z] == Terrain.UpSlopeTile) && Game.World.Covers[x, y, z] == Cover.NoCover && Game.World.Features[x, y, z]==null)
                        {
                            if (Game.World.Random.Next(chance)==0)
                            {
                                Game.World.Covers[x, y, z] = Cover.Grass;
                            }
                        }
                    }
                }
            }
            return ge;
        }
    }
}


//onTurnBegin: function()
//{
//    if (HTomb.Time.dailyCycle.turn % 50 !== 0)
//    {
//        return;
//    }
//    for (let x = 1; x < LEVELW - 1; x++)
//    {
//        for (let y = 1; y < LEVELH - 1; y++)
//        {
//            if (ROT.RNG.getUniform() >= 0.1)
//            {
//                continue;
//            }
//            let z = HTomb.Tiles.groundLevel(x, y);
//            // don't grow over slopes or features I guess
//            if (HTomb.World.tiles[z][x][y] !== HTomb.Tiles.FloorTile || HTomb.World.covers[z][x][y] !== HTomb.Covers.NoCover || HTomb.World.features[coord(x, y, z)])
//            {
//                continue;
//            }

//            if (z === 54)
//            {
//                if (ROT.RNG.getUniformInt(1, 2) === 1)
//                {
//                    var n = HTomb.Tiles.countNeighborsWhere(x, y, z, function(x, y, z) {
//                        return (HTomb.World.covers[z][x][y] === HTomb.Covers.Grass);
//                    });
//    if (n > 0)
//    {
//        HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
//    }
//} else {
//              HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
//            }
//          }
//          if (z<54) {
//            var n = HTomb.Tiles.countNeighborsWhere(x, y, z, function(x, y, z) {
//              return (HTomb.World.covers[z][x][y] === HTomb.Covers.Grass);
//            });
//            if (n>0) {
//              HTomb.World.covers[z][x][y] = HTomb.Covers.Grass;
//            }
//          } else {
//            HTomb.World.covers[z][x][y] = HTomb.Covers.Snow;
//          }
//        }