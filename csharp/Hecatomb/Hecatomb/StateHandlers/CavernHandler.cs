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

    public class Cavern
    {
        public Dictionary<Coord, int> Zones;
        public Cavern()
        {
            Zones = new Dictionary<Coord, int>();
        }
    }


    public class CavernHandler : StateHandler
    {
        public List<Cavern> Caverns;
        
        public CavernHandler()
        {
            Caverns = new List<Cavern>();
        }

        public void DigCaverns(int z)
        {
            var cavern = new Cavern();
            var cells = new CellularAutomaton(Game.World.Width, Game.World.Height);
            cells.Initialize(p: 0.4);
            cells.DoSteps(birth: 45678, steps: 2);
            for (int x = 1; x < Game.World.Width - 1; x++)
            {
                for (int y = 1; y < Game.World.Height - 1; y++)
                {
                    if (!cells.Cells[x, y, 0])
                    {
                        Game.World.Terrains[x, y, z] = Terrain.FloorTile;
                        Game.World.Covers[x, y, z] = Cover.NoCover;
                    }
                }
            }
            int zone = 0;

            // define a recursive flood-fill function
            Action<int, int> flood = null;
            flood = (int x, int y) =>
            {
                cavern.Zones[new Coord(x, y, z)] = zone;
                var neighbors = Tiles.GetNeighbors8(x, y, z);
                foreach (var n in neighbors)
                {
                    if (!Game.World.Terrains[n.X, n.Y, n.Z].Solid && !cavern.Zones.ContainsKey(n))
                    {
                        flood(n.X, n.Y);
                    }
                }
            };

            // fill out the zones...there's a problem in that you can't breach two zones at once...
            for (int x = 1; x < Game.World.Width - 1; x++)
            {
                for (int y = 1; y < Game.World.Height - 1; y++)
                {
                    var c = new Coord(x, y, z);
                    if (!Game.World.Terrains[x, y, z].Solid && !cavern.Zones.ContainsKey(c))
                    {
                        flood(x, y);
                        zone += 1;
                    }
                }
            }
            Debug.WriteLine($"Cavern level {z} contains {zone} distinct zones");
        }
    }
}
