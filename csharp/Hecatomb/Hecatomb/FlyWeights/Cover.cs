/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 12:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
    /// <summary>
    /// Description of Terrain.
    /// </summary>
    public class Cover : FlyWeight<Cover>
    {
        public readonly string Name;
        public readonly char Symbol;
        public readonly string FG;
        public readonly string BG;
        public readonly string DarkBG;
        public readonly bool Solid;
        public readonly bool Liquid;
        public readonly int Hardness;
        public readonly string Mineral;


        public Cover(
            string type,
            string name = "",
            string fg = "white",
            string bg = "black",
            string darkbg = null,
            char symbol = ' ',
            bool liquid = false,
            bool solid = false,
            int hardness = 0,
            string mineral = null
        ) : base(type)
        {
            Name = name;
            FG = fg;
            BG = bg;
            DarkBG = darkbg ?? "red";
            Symbol = symbol;
            Liquid = liquid;
            Solid = solid;
            Hardness = hardness;
            Mineral = mineral;
        }

        public string Shimmer()
        {
            var c = Game.Colors[BG];
            int r = c.R;
            int g = c.G;
            int b = c.B;
            r = (int)Game.World.Random.StatelessNormal(r, r / 16f);
            g = (int)Game.World.Random.StatelessNormal(g, g / 16f);
            b = (int)Game.World.Random.StatelessNormal(b, b / 16f);
            return ("#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2"));
        }
        public static readonly Cover NoCover = new Cover(
            type: "NoCover",
            name: "no cover",
            fg: "black",
            bg: "black"
        );

        public static readonly Cover Grass = new Cover(
            type: "Grass",
            symbol: '"',
            name: "grass",
            fg: "GRASSFG",
            bg: "GRASSBG",
            darkbg: "DARKGRASS"
        );



        public static readonly Cover Water = new Cover(
            type: "Water",
            symbol: '~',
            name: "water",
            fg: "WATERFG",
            bg: "WATERBG",
            darkbg: "DARKWATER",
            liquid: true
        );

        public static readonly Cover Soil = new Cover(
            type: "Soil",
            name: "soil",
            symbol: '#',
            fg: "#BBBBAA",
            bg: "#888877",
            solid: true
        );

        public static readonly Cover Limestone = new Cover(
            type: "Limestone",
            name: "limestone",
            symbol: '#',
            fg: "#999999",
            bg: "#666666",
            solid: true,
            hardness: 1
        );

        public static readonly Cover Basalt = new Cover(
            type: "Basalt",
            name: "basalt",
            symbol: '#',
            fg: "#8888CC",
            bg: "#666699",
            solid: true,
            hardness: 2
        );

        public static readonly Cover Granite = new Cover(
            type: "Granite",
            name: "granite",
            symbol: '#',
            fg: "#AA9999",
            bg: "#776666",
            solid: true,
            hardness: 3
        );

        public static readonly Cover Bedrock = new Cover(
            type: "Bedrock",
            name: "bedrock",
            symbol: '#',
            fg: "#778877",
            bg: "#445544",
            solid: true,
            hardness: 4
        );

        public static readonly Cover CoalSeam = new Cover(
            type: "CoalSeam",
            symbol: '\u26EC',
            name: "coal seam",
            fg: "#222222",
            bg: Soil.BG,
            solid: true,
            mineral: "Coal",
            hardness: 0
        );

        public static readonly Cover FlintCluster = new Cover(
            //symbol: '\u2234',
            symbol: '\u26EC',
            type: "FlintCluster",
            name: "flint cluster",
            fg: "#DDDDDD",
            bg: Soil.BG,
            solid: true,
            mineral: "Flint",
            hardness: 0
        );

        public static readonly Cover CopperVein = new Cover(
            symbol: '\u26EC',
            type: "CopperVein",
            name: "copper vein",
            fg: "#FF9900",
            bg: Limestone.BG,
            solid: true,
            mineral: "CopperOre",
            hardness: 1
        );

        public static readonly Cover TinVein = new Cover(
            symbol: '\u26EC',
            type: "TinVein",
            name: "tin vein",
            fg: "#99FF00",
            bg: Limestone.BG,
            solid: true,
            mineral: "TinOre",
            hardness: 1
        );

        public static readonly Cover IronVein = new Cover(
            symbol: '\u26EC',
            type: "IronVein",
            name: "iron vein",
            fg: "#FF3300",
            bg: Basalt.BG,
            solid: true,
            mineral: "IronOre",
            hardness: 2
        );

        public static readonly Cover SilverVein = new Cover(
            symbol: '\u26EC',
            type: "SilverVein",
            name: "silver vein",
            fg: "#99BBFF",
            bg: Basalt.BG,
            solid: true,
            mineral: "SilverOre",
            hardness: 2
        );

        public static readonly Cover GoldVein = new Cover(
            symbol: '\u26EC',
            type: "GoldVein",
            name: "gold vein",
            fg: "#EEDD00",
            bg: Granite.BG,
            solid: true,
            mineral: "GoldOre",
            hardness: 3
        );

        public static readonly Cover TitaniumVein = new Cover(
            symbol: '\u26EC',
            type: "TitaniumVein",
            name: "titanium vein",
            fg: "#00BB66",
            bg: Granite.BG,
            solid: true,
            mineral: "TitaniumOre",
            hardness: 3
        );

        public static readonly Cover CobaltVein = new Cover(
            symbol: '\u26EC',
            type: "CobaltVein",
            name: "cobalt vein",
            fg: "#4444FF",
            bg: Granite.BG,
            solid: true,
            mineral: "CobaltOre",
            hardness: 3
        );

        public static readonly Cover AdamantVein = new Cover(
            symbol: '\u26EC',
            type: "AdamantVein",
            name: "adamant vein",
            fg: "#FF00FF",
            bg: Bedrock.BG,
            solid: true,
            mineral: "AdamantOre",
            hardness: 4
        );

        public static readonly Cover ThoriumVein = new Cover(
            symbol: '\u26EC',
            type: "ThoriumVein",
            name: "thorium vein",
            fg: "#FFFFFF",
            bg: Bedrock.BG,
            solid: true,
            mineral: "ThoriumOre",
            hardness: 4
        );

        public static readonly Cover TiledStone = new Cover(
            type: "TiledStone",
            symbol: '"',
            name: "tiled stone",
            fg: "FLOORFG",
            bg: "FLOORBG",
            darkbg: "BELOWFG"
        );
        public void Mine(int x, int y, int z)
        {
            if (this.Mineral!=null)
            {
                Item.PlaceNewResource(this.Mineral, 1, x, y, z);
            }
            else if (this.Solid)
            {
                if (Game.World.Random.Next(4)==0)
                {
                    Item.PlaceNewResource("Rock", 1, x, y, z);
                }
            }
            if (!Game.World.Covers[x, y, z].Liquid) // not sure how to deal with liquids here
            {
                Game.World.Covers[x, y, z] = Cover.NoCover;
            }
        }
    }
}
