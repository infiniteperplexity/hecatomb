using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Sapling : Feature
    {
        int Turns;
        public Sapling()
        {
            _name = "sapling";
            _fg = "#55A033";
            _symbol = '\u2698';
            Components.Add(new Harvestable());
            AddListener<TurnBeginEvent>(OnTurnBegin);
            Turns = 50;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Spawned && Placed)
            {
                Turns -= 1;
                if (Turns <= 0)
                {
                    var (x, y, z) = GetPlacedCoordinate();
                    Despawn();
                    if (GameState.World!.Random.Next(2)==0)
                    {
                        Entity.Spawn<ClubTree>().PlaceInValidEmptyTile(x, y, z);
                    }
                    else
                    {
                        Entity.Spawn<SpadeTree>().PlaceInValidEmptyTile(x, y, z);
                    }
                }
            }
            return ge;
        }
    }
}
