using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    class RaidHandler : StateHandler
    {
        public World MainWorld;
        // Serialization will probably be a nightmare, but I'm not going to worry about that for now

        public void LaunchRaid()
        {
            int Width = 64;
            int Height = 64;
            int Depth = 3;
            MainWorld = OldGame.World;
            World raidWorld = new World(Width, Height, Depth);
            var terrains = raidWorld.Terrains;
            var covers = raidWorld.Covers;
            for (int i=0; i<Width; i++)
            {
                for (int j=0; j<Height; j++)
                {
                    if (i == 0 || i == Width - 1 || j == 0 || j == Height - 1)
                    {
                        terrains[i, j, 0] = Terrain.VoidTile;
                        terrains[i, j, 1] = Terrain.VoidTile;
                        terrains[i, j, 2] = Terrain.VoidTile;
                    }
                    else
                    {
                        terrains[i, j, 0] = Terrain.WallTile;
                        terrains[i, j, 1] = Terrain.FloorTile;
                        terrains[i, j, 2] = Terrain.EmptyTile;
                    }
                }
            }
            // Entities is actually set up wrong; it's not attached to the world
            raidWorld.Random = MainWorld.Random;
            raidWorld.Turns.Turn = MainWorld.Turns.Turn;
            raidWorld.Player = MainWorld.Player;
            //raidWorld.StateHandlers["RaidHandler"] = this;
            OldGame.World.Player.Remove();
            OldGame.World = raidWorld;
            raidWorld.Player.Place(32, 32, 1);
            foreach (Creature cr in MainWorld.GetState<TaskHandler>().Minions)
            {
                //cr.PlaceNear(32, 32, 1);
            }
            TurnHandler.HandleVisibility();
        }
    }
}
