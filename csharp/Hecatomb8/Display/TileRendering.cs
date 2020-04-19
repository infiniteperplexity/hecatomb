using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    public static partial class Tiles
    {
        public static string GetBackgroundWithBoundsChecked(int x, int y, int z)
        {
            Creature? c = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z);
            if (c?.BG != null)
            {
                return c.BG!;
            }
            else
            {
                return GameState.World!.Terrains.GetWithBoundsChecked(x, y, z).BG;
            }
        }

        public static string GetForegroundWithBoundsChecked(int x, int y, int z)
        {
            Creature? c = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z);
            if (c?.FG != null)
            {
                return c.FG!;
            }
            else
            {
                return GameState.World!.Terrains.GetWithBoundsChecked(x, y, z).FG;
            }
        }

        public static char GetSymbolWithBoundsChecked(int x, int y, int z)
        {
            Creature? c = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z);
            if (c?.Symbol != null)
            {
                return c.Symbol;
            }
            else
            {
                return GameState.World!.Terrains.GetWithBoundsChecked(x, y, z).Symbol;
            }
        }

        public static (char, string, string) GetGlyphWithBoundsChecked(int x, int y, int z)
        {
            return (GetSymbolWithBoundsChecked(x, y, z), GetForegroundWithBoundsChecked(x, y, z), GetBackgroundWithBoundsChecked(x, y, z));
        }
    }
}
