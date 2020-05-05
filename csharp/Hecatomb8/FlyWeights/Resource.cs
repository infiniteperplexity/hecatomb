using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public class Resource : FlyWeight<Resource>
    {
        [JsonIgnore] public string Name;
        [JsonIgnore] public char Symbol;
        [JsonIgnore] public string FG;
        [JsonIgnore] public string? BG;
        [JsonIgnore] public string TextColor;
        [JsonIgnore] public int StackSize;

        public Resource(
            string type = "",
            string name = "",
            char symbol = ' ',
            string fg = "white",
            int stack = 5,
            string? bg = null,
            string? textColor = null
        ) : base(type)
        {
            Name = name;
            Symbol = symbol;
            FG = fg;
            BG = bg;
            StackSize = stack;
            TextColor = (textColor == null) ? fg : textColor;
        }


        public static string Format((Resource, int) vt)
        {
            var (r, i) = vt;
            return (i + " " + r.Name);
        }
        public static string Format(Dictionary<Resource, int> d)
        {
            if (d.Count == 0)
            {
                return "";
            }
            List<Resource> list = d.Keys.ToList();
            string s = Format((list[0], d[list[0]]));
            for (int i = 1; i < list.Count; i++)
            {
                s += (", " + Format((list[i], d[list[i]])));
            }
            return s;
        }

        public static readonly Resource Rock = new Resource(
            type: "Rock",
            name: "rock",
            symbol: '\u2022',
            fg: "gray"
        );

        public static readonly Resource Flint = new Resource(
            type: "Flint",
            name: "flint",
            symbol: '\u2022',
            fg: "#CCCCCC"
        );

        public static readonly Resource Coal = new Resource(
            type: "Coal",
            name: "coal",
            symbol: '\u2022',
            fg: "#222222",
            textColor: "#666666"
        );

        public static readonly Resource Wood = new Resource(
            type: "Wood",
            name: "wood",
            symbol: '=',
            fg: "brown"
        );
        public static readonly Resource TradeGoods = new Resource(
            type: "TradeGoods",
            name: "trade goods",
            symbol: '\u2696',
            fg: "#AAAA44"
        );
        public static readonly Resource Gold = new Resource(
            type: "Gold",
            name: "gold",
            symbol: '$',
            fg: "#EEDD00"
        );
        public static readonly Resource Silk = new Resource(
            type: "Silk",
            name: "silk",
            symbol: '&',
            fg: "#DDDDDD"
        );

        public static readonly Resource Ectoplasm = new Resource(
            type: "Ectoplasm",
            name: "ectoplasm",
            symbol: '\u2697',
            fg: "magenta"
        );

        public static readonly Resource Flesh = new Resource(
            type: "Flesh",
            name: "flesh",
            symbol: '%',
            fg: "red"
        );

        public static readonly Resource Bone = new Resource(
            type: "Bone",
            name: "bone",
            symbol: '%',
            fg: "white"
        );

        public static readonly Resource Corpse = new Resource(
            type: "Corpse",
            name: "corpse",
            symbol: '%',
            fg: "brown",
            stack: 1
        );

        public static readonly Resource TinOre = new Resource(
            type: "TinOre",
            name: "tin ore",
            symbol: '\u2022',
            fg: "#99FF00"
        );
        public static readonly Resource CopperOre = new Resource(
            type: "CopperOre",
            name: "copper ore",
            symbol: '\u2022',
            fg: "#FF9900"
        );
        public static readonly Resource IronOre = new Resource(
            type: "IronOre",
            name: "iron ore",
            symbol: '\u2022',
            fg: "#FF3300"
        );
        public static readonly Resource SilverOre = new Resource(
            type: "SilverOre",
            name: "silver ore",
            symbol: '\u2022',
            fg: "#99BBFF"
        );
        public static readonly Resource GoldOre = new Resource(
            type: "GoldOre",
            name: "gold ore",
            symbol: '\u2022',
            fg: "#EEDD00"
        );
        public static readonly Resource TitaniumOre = new Resource(
            type: "TitaniumOre",
            name: "titanium ore",
            symbol: '\u2022',
            fg: "#00BB66"
        );
        public static readonly Resource CobaltOre = new Resource(
            type: "CobaltOre",
            name: "cobalt ore",
            symbol: '\u2022',
            fg: "#4444FF"
        );
        public static readonly Resource ThoriumOre = new Resource(
            type: "ThoriumOre",
            name: "thorium ore",
            symbol: '\u2022',
            fg: "#FFFFFF"
        );
        public static readonly Resource AdamantOre = new Resource(
            type: "Adamant Ore",
            name: "adamant ore",
            symbol: '\u2022',
            fg: "#FF00FF"
        );


        public static readonly Resource Undye = new Resource(
            type: "Undye",
            name: "undye"
        );
        public static readonly Resource BloodWort = new Resource(
            type: "BloodWort",
            name: "bloodwort",
            fg: "BloodWort",
            symbol: '\u2698'
        );
        public static readonly Resource Hyacinth = new Resource(
            type: "Hyacinth",
            name: "hyacinth",
            fg: "Hyacinth",
            symbol: '\u2698'
        );
        public static readonly Resource Nightshade = new Resource(
            type: "Nightshade",
            name: "nightshade",
            fg: "Nightshade",
            symbol: '\u2698'
        );
        public static readonly Resource Asphodel = new Resource(
            type: "Asphodel",
            name: "asphodel",
            fg: "Asphodel",
            symbol: '\u2698'
        );
        public static readonly Resource Wolfsbane = new Resource(
            type: "Wolfsbane",
            name: "wolfsbane",
            fg: "Wolfsbane",
            symbol: '\u2698'
        );
        public static readonly Resource WitchHazel = new Resource(
            type: "WitchHazel",
            name: "witch hazel",
            fg: "WitchHazel",
            symbol: '\u2698'
        );
        public static readonly Resource SpiderLily = new Resource(
            type: "SpiderLily",
            name: "spider lily",
            fg: "SpiderLily",
            symbol: '\u2698'
        );
        public static readonly Resource GhostOrchid = new Resource(
            type: "GhostOrchid",
            name: "ghost orchid",
            fg: "GhostOrchid",
            symbol: '\u2698'
        );
        public static readonly Resource MorningGlory = new Resource(
            type: "MorningGlory",
            name: "morning glory",
            fg: "MorningGlory",
            symbol: '\u2698'
        );
        public static List<Resource> Flowers = new List<Resource>
        {
            BloodWort,
            Hyacinth,
            Nightshade,
            Asphodel,
            Wolfsbane,
            WitchHazel,
            SpiderLily,
            GhostOrchid,
            MorningGlory
        };
    }
}
