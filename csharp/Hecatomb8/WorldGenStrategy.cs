using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class WorldGenStrategy
    {
        public void Generate()
        {
            GameState.Player = Entity.Spawn<TileEntity>();
            GameState.Player.Place(1, 1, 1);
            GameState.WorldExists = true;
            InterfaceState.ReadyForInput = true;
        }
    } 
}
