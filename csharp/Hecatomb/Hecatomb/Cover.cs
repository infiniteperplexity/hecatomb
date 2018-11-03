﻿/*
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
        public readonly bool Solid;
        public readonly bool Liquid;
        public readonly int Hardness;
        public readonly string Mineral;


        public Cover(
            string name,
            string fg = "white",
            string bg = "black",
            char symbol = ' ',
            bool liquid = false,
            bool solid = false,
            int hardness = 0,
            string mineral = null
        ) : base()
        {
            Name = name;
            FG = fg;
            BG = bg;
            Symbol = symbol;
            Liquid = liquid;
            Solid = solid;
            Hardness = hardness;
            Mineral = mineral;
        }

        public static readonly Cover NoCover = new Cover(
            name: "no cover",
            fg: "black",
            bg: "black"
        );

        public static readonly Cover Grass = new Cover(
            symbol: '"',
            name: "grass",
            fg: "GRASSFG",
            bg: "GRASSBG"
        );

        public static readonly Cover Water = new Cover(
            symbol: '~',
            name: "water",
            fg: "blue",
            bg: "blue",
            liquid: true
        );

        public static readonly Cover Soil = new Cover(
            name: "soil",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true
        );

        public static readonly Cover Limestone = new Cover(
            name: "limestone",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 1
        );

        public static readonly Cover Basalt = new Cover(
            name: "basalt",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 2
        );

        public static readonly Cover Granite = new Cover(
            name: "granite",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 3
        );

        public static readonly Cover Bedrock = new Cover(
            name: "bedrock",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            hardness: 4
        );

        public static readonly Cover CoalSeam = new Cover(
            symbol: '\u2234',
            name: "coal seam",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            mineral: "Coal"
        );

        public static readonly Cover FlintCluster = new Cover(
            symbol: '\u2234',
            name: "flint cluster",
            fg: "WALLFG",
            bg: "WALLBG",
            solid: true,
            mineral: "Flint"
        );


    }
}
