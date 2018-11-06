using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hecatomb
{
    public partial class Tiles
    {

        public static (char, string) GetColoredSymbol(int x, int y, int z)
        {
            var Creatures = Game.World.Creatures;
            var Items = Game.World.Items;
            var Features = Game.World.Features;
            var Tiles = Game.World.Tiles;
            Terrain terrain = Tiles[x, y, z];
            Cover cover = Game.World.Covers[x, y, z];
            Cover coverb = Game.World.Covers[x, y, z - 1];
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle particle = (pl.Count > 0) ? pl[0] : null;
            Coord c = new Coord(x, y, z);
            Coord ca = new Coord(x, y, z + 1);
            Coord cb = new Coord(x, y, z - 1);
            bool visible = Game.Visible.Contains(c);
            bool va = Game.Visible.Contains(ca);
            bool vb = Game.Visible.Contains(cb);
            int zview = terrain.ZView;
            char sym;
            string fg = null;
            if (!Game.World.Explored.Contains(c))
            {
                // unexplored tiles with an explored floor tile above are rendered as unexplored wall tiles
                if (Game.World.Tiles[x, y, z + 1] == Terrain.FloorTile && Game.World.Explored.Contains(new Coord(x, y, z + 1)))
                {
                    return (Terrain.WallTile.Symbol, "SHADOWFG");
                }
                // otherwise render blank
                else
                {
                    return (' ', "black");
                }
            }
            if (particle!=null && particle.Symbol!=default(char))
            {
                return (particle.Symbol, particle.FG);
            }
            // if the tile is not visible, the foreground will be in shadow
            if (!visible)
            {
                fg = "SHADOWFG";
            }
            // a visible creature
            if (Creatures[c] != null && visible)
            {
                sym = Creatures[c].Symbol;
                fg = fg ?? Creatures[c].FG;
            }
            // a visible creature above
            else if (zview == +1 && Creatures[ca] != null && (visible || va))
            {
                sym = Creatures[ca].Symbol;
                fg = fg ?? "WALLFG";
            }
            // a visible creature below
            else if (zview == -1 && Creatures[cb] != null && (visible || vb))
            {
                sym = Creatures[cb].Symbol;
                // a submerged creature below
                if (coverb.Liquid)
                {
                    fg = fg ?? coverb.FG;
                }
                else
                {
                    fg = fg ?? "BELOWFG";
                }
            }
            // items
            else if (Items[c] != null)
            {
                var tuple = Items[c].GetGlyph();
                sym = tuple.Item1;
                fg = tuple.Item2;
            }
            // features
            else if (Features[c] != null)
            {
                sym = Features[c].Symbol;
                fg = fg ?? Features[c].FG;
            }
            // items above
            else if (zview == +1 && Items[ca] != null)
            {
                sym = Items[ca].GetGlyph().Item1;
                fg = fg ?? "WALLFG";
            }
            // items below
            else if (zview == -1 && Items[cb] != null)
            {
                sym = Items[cb].GetGlyph().Item1;
                // submerged item
                if (coverb.Liquid)
                {
                    fg = coverb.FG;
                }
                else
                {
                    fg = fg ?? "BELOWFG";
                }
            }
            // feature above
            else if (zview == +1 && Features[ca] != null)
            {
                sym = Features[ca].Symbol;
                fg = fg ?? "WALLFG";
            }
            // feature below
            else if (zview == -1 && Features[cb] != null)
            {
                sym = Features[cb].Symbol;
                // submerged feature
                if (coverb.Liquid)
                {
                    fg = coverb.FG;
                }
                else
                {
                    fg = fg ?? "BELOWFG";
                }
            }
            else
            {
                // tile contains no entities, split logic for Symbol and FG
                // *** Symbol ***

                // explored tunnel below a floor
                if (terrain == Terrain.FloorTile && Game.World.Explored.Contains(cb) && !Tiles[x, y, z - 1].Solid)
                {
                    sym = '\u25E6';
                }
                // roof above floor or empty tile
                if ((terrain == Terrain.FloorTile || terrain == Terrain.EmptyTile) && Tiles[x, y, z + 1] != Terrain.EmptyTile)
                {
                    sym = '\'';
                }
                // liquid on the current level
                else if (cover.Liquid)
                {
                    // deeper liquid
                    if (zview == -1 && coverb.Liquid && Tiles[x, y, z - 1] != Terrain.UpSlopeTile)
                    {
                        sym = '\u2235';
                    }
                    // submerged terrain
                    else
                    {
                        sym = terrain.Symbol;
                    }
                }
                // no cover
                else if (cover == Cover.NoCover)
                {
                    // surface of liquid
                    if (zview == -1 && coverb.Liquid)
                    {
                        sym = coverb.Symbol;
                    }
                    // plain terrain
                    else
                    {
                        sym = terrain.Symbol;
                    }
                }
                // minerals, earths, and grassy floors
                else if (cover.Solid || terrain.Solid || terrain==Terrain.FloorTile)
                {
                    sym = cover.Symbol;
                }
                // terrain overriding grass, mostly
                else
                {
                    sym = terrain.Symbol;
                }

                // *** Foreground Color ***
                // no cover on the current tile
                if (cover == Cover.NoCover)
                {
                    // ground with noteworthy cover below
                    if (terrain == Terrain.FloorTile || terrain == Terrain.DownSlopeTile) 
                    {
                        // waterlogged ground or mineral-laden ground
                        if (coverb.Liquid || coverb.Mineral!=null)
                        {
                            fg = fg ?? coverb.FG;
                        }
                        // background color of earth below
                        else if (coverb.Solid)
                        {
                            fg = fg ?? coverb.BG;
                        }
                        else
                        {
                            fg = fg ?? terrain.FG;
                        }

                    }
                    else
                    {
                        fg = fg ?? terrain.FG;
                    }
                }
                // cover overrides terrain for liquids, solids, and floor tiles; e.g. not a grassy slope
                else if (cover.Liquid || cover.Solid || terrain == Terrain.FloorTile)
                {
                    fg = fg ?? cover.FG;
                }
                else
                {
                    fg = fg ?? terrain.FG;
                }
            }
            if (sym==default(char) || fg == null)
            {
                throw new InvalidOperationException("Got an invalid glyph");
            }
            return (sym, fg);
        }

        public static char GetSymbol(int x, int y, int z)
        {
            Creature cr = Game.World.Creatures[x, y, z];
            Feature fr = Game.World.Features[x, y, z];
            Terrain t = Game.World.Tiles[x, y, z];
            Item it = Game.World.Items[x, y, z];
            Cover cv = Game.World.Covers[x, y, z];
            Cover cb = Game.World.Covers[x, y, z - 1];
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle p = (pl.Count > 0) ? pl[0] : null;
            var c = new Coord(x, y, z);
            // won't handle particles the same as in the JS code
            if (p != null && p.Symbol != default(char))
            {
                return p.Symbol;
            }
            // should include debug logic as well
            if (!Game.World.Explored.Contains(c))
            {
                // uneexplored tiles with an explored floor tile above are rendered as unexplored wall tiles
                if (Game.World.Tiles[x, y, z + 1] == Terrain.FloorTile && Game.World.Explored.Contains(new Coord(x, y, z + 1)))
                {
                    return Terrain.WallTile.Symbol;
                }
                else
                {

                }
                return ' ';
            }
            else if (!Game.Visible.Contains(c))
            {
                if (it != null)
                {
                    return it.Symbol;
                }
                else if (fr != null)
                {
                    return fr.Symbol;
                }
                else if (cv != Cover.NoCover && !cv.Solid)
                {
                    return cv.Symbol;
                }
                else if (cb.Liquid)
                {
                    return cb.Symbol;
                }
                else
                {
                    return t.Symbol;
                }
            }
            else
            {
                if (cr != null)
                {
                    return cr.Symbol;
                }
                else if (it != null)
                {
                    return it.GetGlyph().Item1;
                }
                else if (fr != null)
                {
                    return fr.Symbol;
                }
                else if (cv != Cover.NoCover && !cv.Solid && !cv.Liquid && t == Terrain.FloorTile)
                {
                    return cv.Symbol;
                }
                else if (cb.Liquid)
                {
                    return cb.Symbol;
                }
                else
                {
                    return t.Symbol;
                }
            }
        }

        public static string GetFG(int x, int y, int z)
        {
            Creature cr = Game.World.Creatures[x, y, z];
            Feature fr = Game.World.Features[x, y, z];
            Terrain t = Game.World.Tiles[x, y, z];
            Cover cv = Game.World.Covers[x, y, z];
            Cover cb = Game.World.Covers[x, y, z - 1];
            Item it = Game.World.Items[x, y, z];
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle p = (pl.Count > 0) ? pl[0] : null;
            var c = new Coord(x, y, z);
            if (p != null && p.FG != null)
            {
                return p.FG;
            }
            if (!Game.World.Explored.Contains(c))
            {
                return "black";
            }
            else if (!Game.Visible.Contains(c))
            {
                return "SHADOWFG";
            }
            else if (cr != null)
            {
                return cr.FG;
            }
            else if (it != null)
            {
                return it.GetGlyph().Item2;
            }
            else if (fr != null)
            {
                return fr.FG;
            }
            else if (cv != Cover.NoCover)
            {
                return cv.FG;
            }
            else if (cb != Cover.NoCover && cb.Liquid)
            {
                return cb.FG;
            }
            else
            {
                return t.FG;
            }
        }

        public static string GetBG(int x, int y, int z)
        {
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle p = (pl.Count > 0) ? pl[0] : null;
            Terrain t = Game.World.Tiles[x, y, z];
            Cover cv = Game.World.Covers[x, y, z];
            Cover cb = Game.World.Covers[x, y, z - 1];
            Feature f = Game.World.Features[x, y, z];
            Item it = Game.World.Items[x, y, z];
            var c = new Coord(x, y, z);
            TaskEntity task = Game.World.Tasks[x, y, z];
            if (p != null && p.BG != null)
            {
                return p.BG;
            }
            else if (task != null)
            {
                return "orange";
            }
            else if (!Game.World.Explored.Contains(c))
            {
                return "black";
            }
            else if (!Game.Visible.Contains(c))
            {
                return "black";
            }
            else if (it != null && it.Claims.Count > 0)
            {
                return "white";
            }
            else if (it != null && it.GetGlyph().Item3 != null)
            {
                return it.GetGlyph().Item3;
            }
            else if (f != null && f.BG != null)
            {
                return f.BG;
            }
            else if (cv != Cover.NoCover)
            {
                return cv.BG;
            }
            else if (cb.Liquid)
            {
                return cb.Shimmer();
            }
            else
            {
                return t.BG;
            }
        }

        //public static Tuple<char, string, string> GetGlyph(int x, int y, int z)
        //{
        //	return new Tuple<char, string, string>(GetSymbol(x, y, z), GetFG(x, y, z), GetBG(x, y, z));
        //}

        public static (char, string, string) GetGlyph(int x, int y, int z)
        {
            var tuple = GetColoredSymbol(x, y, z);
            return (tuple.Item1, tuple.Item2, GetBG(x, y, z));
            //return (GetSymbol(x, y, z), GetFG(x, y, z), GetBG(x, y, z));
        }
    }
}
