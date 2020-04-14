using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    static class GameState
    {
        // a locator class for global game state stuff
        // nothing should alter the game world without going through here
        
        public static bool WorldExists;
        public static Dictionary<int, Entity> Entities = new Dictionary<int, Entity>();
        public static TileEntity? Player;


        // what if we went crazy and modified this only with a single method called Update()?
        // i think the better way to do this to not set public instance variables
        public static void Update()
        {
        }
    }
}
