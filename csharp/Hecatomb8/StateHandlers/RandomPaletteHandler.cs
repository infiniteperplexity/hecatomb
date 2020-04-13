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
        public static Dictionary<string, string> FlowerDictionary;

        static RandomPaletteHandler()
        {
            FlowerDictionary = new Dictionary<string, string>();
            foreach (var tuple in FlowerNames)
            {
                FlowerDictionary[tuple.Item1] = tuple.Item2;
            }
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

        public RandomPaletteHandler()
        {
            FlowerColors = new Dictionary<string, string>();
        }

        public void PickFlowerColors()
        {
            Debug.WriteLine("picking flower colors");
            List<int> picks = new List<int>();
            int tries = 0;
            int stopChecking = 1000;
            while (picks.Count< 9)
            {
                int i = OldGame.World.Random.Next(PossibleFlowerColors.Count);
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

        public string GetFlowerName(string s)
        {
            return FlowerDictionary[s];
        }

        public static Feature SpawnFlower(string s)
        {
            Feature f = Entity.Spawn<Feature>("Flower");
            f.GetComponent<RandomPaletteComponent>().RandomPaletteType = s;
            f.GetComponent<Harvestable>().Yields[s] = 1;
            return f;
        }

        // this is so ad hoc...
        public static Feature MockFlower(string s)
        {
            Feature f = Entity.Mock<Feature>("Flower");
            f.Name = FlowerDictionary[s];
            return f;
        }
    }
}
