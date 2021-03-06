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
	public class Terrain : FlyWeight<Terrain>
	{
        public readonly string Name;
		public readonly char Symbol;
		public readonly string FG;
		public readonly	string BG;
		public readonly bool Opaque;
		public readonly bool Solid;
		public readonly bool Fallable;
		public readonly bool Mutable;
		public readonly int ZView;
		public readonly int ZWalk;

		
		public Terrain(
            string type = "",
			string name = "",
			string fg = "white",
			string bg = "black",
			char symbol = ' ',
			bool opaque = false,
			bool solid = false,
			bool fallable = false,
			bool mutable = true,
			int zview = 0,
			int zwalk = 0
		) : base(type)
		{
            Name = name;
			FG = fg;
			BG = bg;
			Symbol = symbol;
			Opaque = opaque;
			Solid = solid;
			Fallable = fallable;
			Mutable = mutable;
			ZView = zview;
			ZWalk = zwalk;
		}
		
		public static readonly Terrain VoidTile = new Terrain(
            type: "VoidTile",
			name: "boundary",
			fg: "black",
			bg: "black",
			opaque: true,
			solid: true,
			mutable: false
		);
		
		public static readonly Terrain EmptyTile = new Terrain(
            type: "EmptyTile",
            name: "empty",
			//symbol: '.',
            symbol: '\u22C5',
			fg: "BELOWFG",
		    bg: "BELOWBG", 
			fallable: true,
			zview: -1
		);
		
		public static readonly Terrain FloorTile = new Terrain(
            type: "FloorTile",
            name: "floor",
			symbol: '.',
			fg: "FLOORFG",
			bg: "FLOORBG"
		);
		
		public static readonly Terrain WallTile = new Terrain(
            type: "WallTile",
            name: "wall",
			symbol: '#',
			fg: "WALLFG",
			bg: "WALLBG",
			opaque: true,
			solid: true
		);
		
		public static readonly Terrain UpSlopeTile = new Terrain(
            type: "UpSlopeTile",
            name: "upward slope",
			symbol: '^',
			fg: "FLOORFG",
			bg: "FLOORBG",
			zview: +1,
			zwalk: +1	
		);
		
		public static readonly Terrain DownSlopeTile = new Terrain(
            type: "DownSlopeTile",
            name: "downward slope",
			symbol: '\u02C7',
            //symbol: '\u2228',
			fg: "BELOWFG",
			bg: "BELOWBG",
			zview: -1,
			zwalk: -1
		);	
		
		public static readonly Terrain OutOfBoundsTile = new Terrain(
            type: "OutOfBoundsTile",
            name: "out of bounds",
			symbol: ' ',
			fg: "black",
			bg: "black",
			zview: 0,
			zwalk: 0,
			mutable: false
		);	
		
	}
}
