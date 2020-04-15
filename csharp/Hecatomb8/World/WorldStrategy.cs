using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class WorldStrategy
    {
        public void Generate()
        {
            GameState.Player = Entity.Spawn<Creature>();
            var (x, y, z) = (new Constrained<int>(1), new Constrained<int>(1), new Constrained<int>(1));
            if (!GameState.World!.Creatures.ContainsKey(x, y, z))
            {
                GameState.Player.PlaceInEmptyTile(x, y, z);
            }
            else
            {
                throw new Exception("Player can't be placed.");
            }
            InterfaceState.ReadyForInput = true;
        }
    } 
}
