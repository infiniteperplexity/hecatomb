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

        public static (char, string) GetColoredSymbol(int x, int y, int z, bool useLighting = true)
        {
            var Creatures = Game.World.Creatures;
            var Items = Game.World.Items;
            var Features = Game.World.Features;
            var Tiles = Game.World.Terrains;
            Terrain terrain = Tiles[x, y, z];
            Cover cover = Game.World.Covers[x, y, z];
            Cover coverb = Game.World.Covers[x, y, z - 1];
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle particle = (pl.Count > 0) ? pl[0] : null;
            Coord c = new Coord(x, y, z);
            Coord ca = new Coord(x, y, z + 1);
            Coord cb = new Coord(x, y, z - 1);
            bool visible = Game.Visible.Contains(c) || Game.Options.Visible;
            bool va = Game.Visible.Contains(ca) || Game.Options.Visible;
            bool vb = Game.Visible.Contains(cb) || Game.Options.Visible;
            int zview = terrain.ZView;
            char sym;
            string fg = null;
            if (!Game.World.Explored.Contains(c) && !Game.Options.Explored)
            {
                // unexplored tiles with an explored floor tile above are rendered as unexplored wall tiles
                if (Game.World.Terrains[x, y, z + 1] == Terrain.FloorTile && Game.World.Explored.Contains(new Coord(x, y, z + 1)))
                {
                    return (Terrain.WallTile.Symbol, "SHADOWFG");
                }
                // otherwise render blank
                else
                {
                    return (' ', "black");
                }
            }
            if (particle != null && particle.Symbol != default(char))
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
                else if ((terrain == Terrain.FloorTile || terrain == Terrain.EmptyTile) && Tiles[x, y, z + 1] != Terrain.EmptyTile)
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
                else if (cover.Solid || terrain.Solid || terrain == Terrain.FloorTile)
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
                        if (coverb.Liquid || coverb.Mineral != null)
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
            if (sym == default(char) || fg == null)
            {
                throw new InvalidOperationException("Got an invalid glyph");
            }
            if (useLighting)
            {
                int lighting = Game.World.Turns.LightLevel;
                int outdoors = Game.World.Outdoors[x, y, z];
                if (outdoors==0)
                {
                    lighting = 0;
                }
                else if (outdoors==1)
                {
                    lighting = lighting / 2;
                }
                return (sym, Game.Colors.Shade(fg, lighting));
            }
            return (sym, fg);
        }

        public static string GetBackground(int x, int y, int z)
        {
            List<Particle> pl = Game.World.Particles[x, y, z];
            Particle p = (pl.Count > 0) ? pl[0] : null;
            Terrain t = Game.World.Terrains[x, y, z];
            Terrain tb = Game.World.Terrains[x, y, z - 1];
            Cover cv = Game.World.Covers[x, y, z];
            Cover cb = Game.World.Covers[x, y, z - 1];
            Feature f = Game.World.Features[x, y, z];
            Item it = Game.World.Items[x, y, z];
            var c = new Coord(x, y, z);
            Task task = Game.World.Tasks[x, y, z];
            // particle
            if (p != null && p.BG != null)
            {
                return p.BG;
            }
            // this should event
            else if (task != null)
            {
                return (task.BG ?? "orange");
            }
            else if (!Game.World.Explored.Contains(c) && !Game.Options.Explored)
            {
                return "black";
            }
            else if (!Game.Visible.Contains(c) && !Game.Options.Visible)
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
            else if (f != null && (f.BG != null || f.Highlight!=null))
            {
                return f.Highlight ?? f.BG;
            }
            else if (cv != Cover.NoCover)
            {
                return cv.BG;
            }
            else if (t.ZView == -1)
            {
                if (cb.Liquid)
                {
                    return cb.Shimmer();
                }
                else if (cb != Cover.NoCover)
                {
                    return cb.DarkBG;
                }
                else
                {
                    Cover cb2 = Game.World.Covers[x, y, z - 2];
                    if (Game.World.Terrains[x, y, z - 1].ZView ==-1 && cb2.Liquid)
                    {
                        return cb2.DarkBG;
                    }
                    else
                    {
                        return t.BG;
                    }
                }
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
            return (tuple.Item1, tuple.Item2, GetBackground(x, y, z));
            //return (GetSymbol(x, y, z), GetFG(x, y, z), GetBG(x, y, z));
        }
    }
}
