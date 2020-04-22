using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    public static partial class Tiles
    {

        public static (char, string) GetSymbolWithBoundsChecked(int x, int y, int z, bool useLighting = true)
        {
            var cr = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z);
            var cra = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z + 1);
            var crb = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z - 1);
            var fr = GameState.World!.Features.GetWithBoundsChecked(x, y, z);
            var fra = GameState.World!.Features.GetWithBoundsChecked(x, y, z + 1);
            var frb = GameState.World!.Features.GetWithBoundsChecked(x, y, z - 1);
            var it = GameState.World!.Items.GetWithBoundsChecked(x, y, z);
            var ita = GameState.World!.Items.GetWithBoundsChecked(x, y, z + 1);
            var itb = GameState.World!.Items.GetWithBoundsChecked(x, y, z - 1);
            Terrain terrain = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z);
            Terrain ta = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z + 1);
            Terrain tb = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z - 1);
            Cover cover = GameState.World!.Covers.GetWithBoundsChecked(x, y, z);
            Cover coverb = GameState.World!.Covers.GetWithBoundsChecked(x, y, z - 1);
            //List<Particle> pl = Game.World.Particles[x, y, z];
            //Particle particle = (pl.Count > 0) ? pl[0] : null;
            Coord c = new Coord(x, y, z);
            Coord ca = new Coord(x, y, z + 1);
            Coord cb = new Coord(x, y, z - 1);
            bool visible = InterfaceState.PlayerVisible.Contains(c) || HecatombOptions.Visible;
            bool va = InterfaceState.PlayerVisible.Contains(ca) || HecatombOptions.Visible;
            bool vb = InterfaceState.PlayerVisible.Contains(cb) || HecatombOptions.Visible;
            int slope = terrain.Slope;
            char sym;
            string? fg = null;

            //var explored = true;
            //var exa = true;
            //var exb = true;

            var explored = GameState.World!.Explored.Contains(c) || HecatombOptions.Explored;
            var exa = GameState.World.Explored.Contains(ca) || HecatombOptions.Explored;
            var exb = GameState.World.Explored.Contains(cb) || HecatombOptions.Explored;

            if (!explored)
            { 
                // unexplored tiles with an explored floor tile above are rendered as unexplored wall tiles
                if (ta == Terrain.FloorTile && exa)
                {
                    return (Terrain.WallTile.Symbol, "SHADOWFG");
                }
                // otherwise render blank
                else
                {
                    return (' ', "black");
                }
            }
            //if (particle != null && particle.Symbol != default(char))
            //{
            //    return (particle.Symbol, particle.FG);
            //}
            // if the tile is not visible, the foreground will be in shadow
            if (!visible)
            {
                fg = "SHADOWFG";
            }

            // a visible creature
            if (cr != null && cr.Symbol != ' ' && visible)
            {
                sym = cr.Symbol;
                fg = fg ?? cr.FG;
            }
            // a visible creature above
            else if (slope == +1 && cra != null && cra.Symbol != ' ' && (visible || va))
            {
                sym = cra.Symbol;
                fg = fg ?? "WALLFG";
            }
            // a visible creature below
            else if (slope == -1 && crb != null &&crb.Symbol != ' ' && (visible || vb))
            {
                sym = crb.Symbol;
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
            else if (it != null)
            {
                sym = it.Symbol;
                fg = it.FG;
            }
            // features
            else if (fr != null && fr.Symbol != ' ' && fr.FG != null)
            {
                sym = fr.Symbol;
                fg = fg ?? fr.FG;
            }
            // used mostly for masonry
            else if (fr != null && fr.Symbol != ' ' && fr.FG == null)
            {
                sym = fr.Symbol;
                if (cover.Liquid)
                {
                    fg = cover.FG;
                }
                else
                {
                    fg = terrain.FG;
                }
            }
            // used mostly for masonry
            else if (fr != null && fr.Symbol == ' ' && fr.FG != null)
            {
                fg = fr.FG;
                if (cover.Liquid)
                {
                    sym = cover.Symbol;
                }
                else
                {
                    sym = terrain.Symbol;
                }
            }
            // items above
            else if (slope == +1 && ita != null)
            {
                sym = ita.Symbol;
                fg = fg ?? "WALLFG";
            }
            // items below
            else if (slope == -1 && itb != null)
            {
                sym = itb.Symbol;
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
            else if (slope == +1 && fra != null && fra.Symbol != ' ')
            {
                sym = fra.Symbol;
                fg = fg ?? "WALLFG";
            }
            // feature belowb
            else if (slope == -1 && frb != null && frb.Symbol != ' ')
            {
                sym = frb.Symbol;
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
                if (terrain == Terrain.FloorTile && exb && !tb.Solid)
                {
                    sym = '\u25E6';
                }
                // roof above floor or empty tile
                else if ((terrain == Terrain.FloorTile || terrain == Terrain.EmptyTile) && ta != Terrain.EmptyTile)
                {
                    sym = '\'';
                }
                // liquid on the current level
                else if (cover.Liquid)
                {
                    // deeper liquid
                    if (slope == -1 && coverb == cover && tb != Terrain.UpSlopeTile)
                    {
                        //sym = '\u2235';
                        sym = '.';
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
                    if (slope == -1 && coverb.Liquid)
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
                        if (coverb.Liquid || (coverb.Solid && coverb.Resource != null))
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
                else if (cover.Solid || terrain == Terrain.FloorTile)
                {
                    fg = fg ?? cover.FG;
                }
                else if (cover.Liquid)
                {
                    if (slope == -1 && coverb == cover)
                    {
                        fg = fg ?? "BELOWFG";
                    }
                    else
                    {
                        fg = fg ?? cover.FG;
                    }
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
            //if (useLighting)
            //{
            //    int lighting = Game.World.GetLighting(x, y, z);
            //    if (!visible)
            //    {
            //        return (sym, fg);
            //    }
            //    return (sym, Game.Colors.Shade(fg, lighting));
            //}
            return (sym, fg);
        }

        public static string GetBackgroundWithBoundsChecked(int x, int y, int z)
        {
            //List<Particle> pl = Game.World.Particles[x, y, z];
            //Particle p = (pl.Count > 0) ? pl[0] : null;

            var cr = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z);
            var fr = GameState.World!.Features.GetWithBoundsChecked(x, y, z);
            var it = GameState.World!.Items.GetWithBoundsChecked(x, y, z);
            Terrain terrain = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z);
            Terrain ta = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z + 1);
            Terrain tb = GameState.World!.Terrains.GetWithBoundsChecked(x, y, z - 1);
            Cover cva = GameState.World!.Covers.GetWithBoundsChecked(x, y, z);
            Cover cvb = GameState.World!.Covers.GetWithBoundsChecked(x, y, z - 1);

            Coord c = new Coord(x, y, z);
            var explored = GameState.World.Explored.Contains(c) || HecatombOptions.Explored;
            //var explored = true;

            //Task task = Game.World.Tasks[x, y, z];
            // particle
            //if (p != null && p.BG != null)
            //{
            //    return p.BG;
            //}
            // this should event
            //else if (task != null)
            //{
            //    return (task.BG ?? task.BG);
            //}
            //else
            if (!explored)
            {
                return "black";
            }
            //else if (!Game.Visible.Contains(c) && !Game.Options.Visible)
            //{
            //    return "black";
            //}
            else if (cr != null && (cr.BG != null))
            {
                return cr.BG;
            }
            else if (it != null && it.Claimed > 0)
            {
                return "white";
            }
            else if (fr != null && (fr.BG != null))
            {
                return fr.BG;
            }
            else if (it != null && it.BG != null)
            {
                return it.BG;
            }
            else if (cva != Cover.NoCover)
            {
                return cva.BG;
            }
            else if (terrain.ZView == -1)
            {
                if (cvb.Liquid)
                {
                    return cvb.FG;
                    //return cb.Shimmer();
                }
                else if (cvb != Cover.NoCover)
                {
                    return cvb.DarkBG;
                }
                else
                {
                    Cover cb2 = GameState.World!.Covers.GetWithBoundsChecked(x, y, z - 2);
                    if (tb.ZView == -1 && cb2.Liquid)
                    {
                        return cb2.DarkBG;
                    }
                    else
                    {
                        return terrain.BG;
                    }
                }
            }
            else
            {
                return terrain.BG;
            }
        }

        public static (char, string, string) GetGlyphWithBoundsChecked(int x, int y, int z)
        {
            var (sym, fg) = GetSymbolWithBoundsChecked(x, y, z);
            var bg = GetBackgroundWithBoundsChecked(x, y, z);
            return (sym, fg, bg);
        }
    }
}
