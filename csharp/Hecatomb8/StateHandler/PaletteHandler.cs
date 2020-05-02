using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    class PaletteHandler : StateHandler
    {
        public Dictionary<string, string> FlowerColors;
        public static List<int> PossibleFlowerColors;

        static PaletteHandler()
        {
            PossibleFlowerColors = new List<int>();
            // consider all possible colors
            for (int i = 0; i < 0xFFFFFF; i++)
            {
                bool tooGreen = (Colors.GetGreenness(i) >= 128);
                bool tooGray = (Colors.GetLightness(i) < 0xBB && Colors.GetBrightness(i) < 50);
                bool tooDark = Colors.GetLightness(i) < 128;
                //f (!tooGray && !tooGreen && !tooDark)
                if (!tooDark)
                {
                    PossibleFlowerColors.Add(i);
                }
            }          
        }

        public PaletteHandler()
        {
            FlowerColors = PickFlowerColors();
        }

        public static Dictionary<string, string> PickFlowerColors()
        {
            List<int> picks = new List<int>();
            int tries = 0;
            int stopChecking = 1000;
            while (picks.Count < 9)
            {
                int i = GameState.World!.Random.Next(PossibleFlowerColors.Count);
                int j = PossibleFlowerColors[i];
                var (r, g, b) = Colors.SplitRGB(j);
                int lowestDifference = 0xFF + 0xFF + 0xFF;
                foreach (int c in picks)
                {
                    var (r1, g1, b1) = Colors.SplitRGB(c);
                    lowestDifference = Math.Min(lowestDifference, Math.Abs(r - r1) + Math.Abs(g - g1) + Math.Abs(b - b1));
                }
                tries += 1;
                if (lowestDifference > 64 || tries > stopChecking)
                {
                    picks.Add(PossibleFlowerColors[i]);
                }
            }
            var colors = new Dictionary<string, string>();
            for (int i = 0; i < Resource.Flowers.Count; i++)
            {
                colors[Resource.Flowers[i].TypeName] = "#" + picks[i].ToString("X6");
            }
            return colors;
        }
    }
}
