﻿using System;
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

    public class LiquidHandler : StateHandler
    {
        // this could handle cave-ins as well
        public HashSet<Coord> NextFloods;
        public HashSet<Coord> OldFloods;
        public LiquidHandler() : base()
        {
            OldFloods = new HashSet<Coord>();
            NextFloods = new HashSet<Coord>();
            AddListener<DigEvent>(OnDig);
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public GameEvent OnDig(GameEvent ge)
        {
            DigEvent de = (DigEvent)ge;
            {
                foreach (Coord c in Tiles.GetNeighbors26(de.X, de.Y, de.Z))
                {
                    if (Covers.GetWithBoundsChecked(c.X, c.Y, c.Z).Liquid)
                    {
                        Flood(c.X, c.Y, c.Z, liquid: Covers.GetWithBoundsChecked(c.X, c.Y, c.Z));
                    }
                }
            }
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            // maybe we need one hashset per liquid?
            OldFloods = new HashSet<Coord>(NextFloods);
            NextFloods.Clear();
            foreach (Coord c in OldFloods)
            {
                Flood(c.X, c.Y, c.Z);
            }
            return ge;
        }

        public void Flood(int x, int y, int z, Cover? liquid = null)
        {
            liquid = liquid ?? Cover.Water;
            //if (Game.World.Covers[x, y, z] == Cover.NoCover)
            //{
            Covers.SetWithBoundsChecked(x, y, z, liquid);
            //}
            var neighbors = Tiles.GetNeighbors6(x, y, z);
            foreach (var tile in neighbors)
            {
                var (x1, y1, z1) = tile;
                // colliding liquids should get handled someday
                if (z1 <= z && !Terrains.GetWithBoundsChecked(x1, y1, z1).Solid && !Covers.GetWithBoundsChecked(x1, y1, z1).Liquid)
                {
                    // flood instantly downward
                    if (z1 < z)
                    {
                        Flood(tile.X, tile.Y, tile.Z);
                    }
                    else
                    // flood one tile per turn on level terrain
                    {
                        NextFloods.Add(tile);
                    }
                }
            }

        }
    }

}
