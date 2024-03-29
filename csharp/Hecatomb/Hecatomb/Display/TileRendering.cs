﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hecatomb
{
    using static HecatombAliases;
    public partial class Tiles
    {
        
        public static (char, string) GetColoredSymbol(int x, int y, int z, bool useLighting = true)
        {
            if (!World.WorldSafeToDraw)
            {
                return (' ', "black");
            }
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
            if (Creatures[c] != null && Creatures[c].GetCalculatedSymbol() != ' ' && visible)
            {
                sym = Creatures[c].GetCalculatedSymbol();
                fg = fg ?? Creatures[c].GetCalculatedFG();
            }
            // a visible creature above
            else if (zview == +1 && Creatures[ca] != null && Creatures[ca].GetCalculatedSymbol() != ' ' && (visible || va))
            {
                sym = Creatures[ca].GetCalculatedSymbol();
                fg = fg ?? "WALLFG";
            }
            // a visible creature below
            else if (zview == -1 && Creatures[cb] != null && Creatures[cb].GetCalculatedSymbol() != ' ' && (visible || vb))
            {
                sym = Creatures[cb].GetCalculatedSymbol();
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
                Resource r = Resource.Types[Items[c].Resource];
                sym = r.Symbol;
                fg = Items[c].GetCalculatedFG();
            }
            // features
            else if (Features[c] != null && Features[c].GetCalculatedSymbol() != ' ' && Features[c].GetCalculatedFG() != null)
            {
                sym = Features[c].GetCalculatedSymbol();
                fg = fg ?? Features[c].GetCalculatedFG();
            }
            // used mostly for masonry
            else if (Features[c] != null && Features[c].GetCalculatedSymbol() != ' ' && Features[c].GetCalculatedFG() == null)
            {
                sym = Features[c].GetCalculatedSymbol();
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
            else if (Features[c] != null && Features[c].GetCalculatedSymbol() == ' ' && Features[c].GetCalculatedFG() != null)
            {
                fg = Features[c].GetCalculatedFG();
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
            else if (zview == +1 && Items[ca] != null)
            {
                Resource r = Resource.Types[Items[ca].Resource];
                sym = r.Symbol;
                fg = fg ?? "WALLFG";
            }
            // items below
            else if (zview == -1 && Items[cb] != null)
            {
                Resource r = Resource.Types[Items[cb].Resource];
                sym = r.Symbol;
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
            else if (zview == +1 && Features[ca] != null && Features[ca].GetCalculatedSymbol() != ' ')
            {
                sym = Features[ca].GetCalculatedSymbol();
                fg = fg ?? "WALLFG";
            }
            // feature belowb
            else if (zview == -1 && Features[cb] != null && Features[cb].GetCalculatedSymbol() != ' ')
            {
                sym = Features[cb].GetCalculatedSymbol();
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
                if (terrain == Terrain.FloorTile && (Game.World.Explored.Contains(cb) || Options.Explored) && !Tiles[x, y, z - 1].Solid)
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
                    if (zview == -1 && coverb == cover && Tiles[x, y, z - 1] != Terrain.UpSlopeTile)
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
                else if (cover.Solid || terrain == Terrain.FloorTile)
                {
                    fg = fg ?? cover.FG;
                }
                else if (cover.Liquid)
                {
                    if (zview == -1 && coverb == cover)
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
            if (useLighting)
            {
                int lighting = Game.World.GetLighting(x, y, z);
                if (!visible)
                {
                    return (sym, fg);
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
            Creature cr = Game.World.Creatures[x, y, z];
            Feature f = Game.World.Features[x, y, z];
            Item it = Game.World.Items[x, y, z];
            var c = new Coord(x, y, z);
            Resource res = (it==null) ? null : res = Resource.Types[Game.World.Items[c].Resource];
            Task task = Game.World.Tasks[x, y, z];
            // particle
            if (p != null && p.BG != null)
            {
                return p.BG;
            }
            // this should event
            else if (task != null)
            {
                return (task.BG ?? task.BG);
            }
            else if (!Game.World.Explored.Contains(c) && !Game.Options.Explored)
            {
                return "black";
            }
            //else if (!Game.Visible.Contains(c) && !Game.Options.Visible)
            //{
            //    return "black";
            //}
            else if (cr != null && (cr.GetCalculatedBG() != null))
            {
                return cr.GetCalculatedBG();
            }
            else if (it != null && it.Claimed > 0)
            {
                return "white";
            }
            else if (f != null && (f.GetCalculatedBG() != null))
            {
                return f.GetCalculatedBG();
            }
            else if (it != null && res.BG != null)
            {
                return res.BG;
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
            if (!World.WorldSafeToDraw)
            {
                return (' ', "black", "black");
            }
            bool useLighting = true;
            Creature c = Creatures[x, y, z];
            if (c!=null && (c==Player || c.GetComponent<Actor>().Team == Teams.Friendly))
            {
                useLighting = false;
            }
            Feature f = Features[x, y, z];
            if (f!=null && f.TryComponent<StructuralComponent>()!=null)
            {
                useLighting = false;
            }
            var tuple = GetColoredSymbol(x, y, z, useLighting: useLighting);
            return (tuple.Item1, tuple.Item2, GetBackground(x, y, z));
            //return (GetSymbol(x, y, z), GetFG(x, y, z), GetBG(x, y, z));
        }
    }
}
