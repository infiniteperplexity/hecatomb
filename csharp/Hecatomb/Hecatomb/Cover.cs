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
    public class Cover : FlyWeight
    {
        public readonly char Symbol;
        public readonly string FG;
        public readonly string BG;
        public readonly string Dark;
        public readonly bool Solid;
        public readonly bool Liquid;
        public readonly int Hardness;
        public readonly string Mineral;


        public Cover(
            string type,
            string name,
            string fg = "white",
            string bg = "black",
            string dark = null,
            char symbol = ' ',
            bool liquid = false,
            bool solid = false,
            int hardness = 0,
            string mineral = null
        ) : base(type, name)
        {
            FG = fg;
            BG = bg;
            Dark = dark; // maybe some default logic here?
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
            r = (int) Game.World.Random.NextNormal(r, r / 16f);
            g = (int) Game.World.Random.NextNormal(g, g / 16f);
            b = (int) Game.World.Random.NextNormal(b, b / 16f);
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
            bg: "GRASSBG"
        );

        public static readonly Cover Water = new Cover(
            type: "Water",
            symbol: '~',
            name: "water",
            fg: "WATERFG",
            bg: "WATERBG",
            liquid: true
        );

        public static readonly Cover Soil = new Cover(
            type: "Soil",
            name: "soil",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true
        );

        public static readonly Cover Limestone = new Cover(
            type: "Limestone",
            name: "limestone",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 1
        );

        public static readonly Cover Basalt = new Cover(
            type: "Basalt",
            name: "basalt",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 2
        );

        public static readonly Cover Granite = new Cover(
            type: "Granite",
            name: "granite",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 3
        );

        public static readonly Cover Bedrock = new Cover(
            type: "Bedrock",
            name: "bedrock",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 4
        );

        public static readonly Cover CoalSeam = new Cover(
            symbol: '\u2234',
            type: "CoalSeam",
            name: "coal seam",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            mineral: "Coal"
        );

        public static readonly Cover FlintCluster = new Cover(
            symbol: '\u2234',
            type: "FlintCluster",
            name: "flint cluster",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            mineral: "Flint"
        );


    }
}
