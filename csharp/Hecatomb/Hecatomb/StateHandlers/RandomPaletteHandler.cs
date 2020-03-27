using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    class RandomPaletteHandler : StateHandler
    {
        public Dictionary<string, string> FlowerColors;
        public static List<int> PossibleFlowerColors;
        public static List<(string, string)> FlowerNames = new List<(string, string)>()
        {
            ("BloodWort", "bloodwort"),
            ("Hyacinth", "hyacinth"),
            ("Nightshade", "nightshade"),
            ("Ghostflower", "ghostflower"),
            ("Asphodel", "asphodel"),
            ("Wolfsbane", "wolfsbane"),
            ("WitchHazel", "witch hazel"),
            ("SpiderLily", "spider lily"),
            ("MorningGlory", "morning glory")
        };

        static RandomPaletteHandler()
        {
            PossibleFlowerColors = new List<int>();
            // consider all possible colors
            for (int i = 0; i < 0xFFFFFF; i++)
            {
                //// we want to get rid of dark grays and muddy browns
                ///// separate the digits into RGB
                int j = i;
                //j = 0x4E4A3A;
                int r = j / 0xFFFF;
                j -= r * 0xFFFF;
                int g = j / 0xFF;
                j -= g * 0xFF;
                int b = j;
                int greenness = 2 * g - r - b;
                bool tooGreen = (greenness >= 100);
                // greatest difference between two component
                int colorness = Math.Max(Math.Max(Math.Abs(r - g), Math.Abs(r - b)), Math.Abs(g - b));
                // overall lightness of color
                int lightness = r + g + b;
                //Debug.WriteLine($"rgb {r} {g} {b}");
                //Debug.WriteLine($"grayness {colorness} {lightness}");
                bool tooGray = (lightness < (0xBB + 0xBB + 0xBB) && colorness < 50);
                if (!tooGray && !tooGreen)
                {
                    PossibleFlowerColors.Add(i);
                }
            }
        }

        public RandomPaletteHandler()
        {
            FlowerColors = new Dictionary<string, string>();
        }

        public void PickFlowerColors()
        {
            List<int> picks = new List<int>();
            int tries = 0;
            int stopChecking = 1000;
            while (picks.Count< 9)
            {
                int i = Game.World.Random.Next(PossibleFlowerColors.Count);
                int j = PossibleFlowerColors[i];
                int r = j / 0xFFFF;
                j -= r;
                int g = j / 0xFF;
                j -= g;
                int b = j;
                int lowestDifference = 0xFF + 0xFF + 0xFF;
                foreach (int c in picks)
                {
                    int j1 = c;
                    int r1 = j1 / 0xFFFF;
                    j1 -= r1;
                    int g1 = j1 / 0xFF;
                    j1 -= g1;
                    int b1 = j1;
                    lowestDifference = Math.Min(lowestDifference, Math.Abs(r - r1) + Math.Abs(g - g1) + Math.Abs(b - b1));
                }
                tries += 1;
                if (lowestDifference > 64 || tries > stopChecking)
                {
                    picks.Add(PossibleFlowerColors[i]);
                }
            }
            for (int i = 0; i < FlowerNames.Count; i++)
            {
                //FlowerColors[FlowerNames[i].Item1] = "#" + Convert.ToString(picks[i],(16)).ToUpper();
                FlowerColors[FlowerNames[i].Item1] = "#" + picks[i].ToString("X6");
            }
        }

        public string GetFlowerColor(string s)
        {
            if (!FlowerColors.ContainsKey(s))
            {
                PickFlowerColors();
            }
            return FlowerColors[s];
        }

        public string GetFlowerColorName(string s)
        {
            foreach (var tuple in RandomPaletteHandler.FlowerNames)
            {
                if (tuple.Item1 == s)
                {
                    return Game.World.GetState<RandomPaletteHandler>().GetFlowerColor(s);
                }
            }
            return "white";
        }
    }
}
