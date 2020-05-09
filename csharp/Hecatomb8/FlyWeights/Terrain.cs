using System;
using Newtonsoft.Json;

namespace Hecatomb8
{
	// Terrain flyweights represent the topology of a single tile - flat, a slope, et cetera.
	public class Terrain : FlyWeight<Terrain>
	{
		[JsonIgnore] public readonly string Name;
		[JsonIgnore] public readonly char Symbol;
		[JsonIgnore] public readonly string FG;
		[JsonIgnore] public readonly string BG;
		[JsonIgnore] public readonly bool Opaque;
		[JsonIgnore] public readonly bool Solid;
		[JsonIgnore] public readonly bool Fallable;
		[JsonIgnore] public readonly bool Mutable;
		[JsonIgnore] public readonly int ZView;
		[JsonIgnore] public readonly bool Floor;
		[JsonIgnore] public readonly int Slope;



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
			bool floor = true,
			int slope = 0
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
			Floor = floor;
			Slope = slope;
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
			symbol: '\u22C5',
			fg: "BELOWFG",
			bg: "BELOWBG",
			fallable: true,
			floor: false
		);

		// A FloorTile should always have a wall or void tile underneath
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

		// An UpSlopeTile will usually have a DownSlopeTile above it and can never have an EmptyTile above it
		public static readonly Terrain UpSlopeTile = new Terrain(
			type: "UpSlopeTile",
			name: "upward slope",
			symbol: '^',
			fg: "FLOORFG",
			bg: "FLOORBG",
			slope: +1
		);

		// A DownSlopeTile will always have an UpSlopeTile beneath it
		public static readonly Terrain DownSlopeTile = new Terrain(
			type: "DownSlopeTile",
			name: "downward slope",
			symbol: '\u02C7',
			fg: "BELOWFG",
			bg: "BELOWBG",
			floor: false,
			slope: -1
		);

		// I could use this as a placeholder in certain circumstances but it's a bad idea
		public static readonly Terrain OutOfBoundsTile = new Terrain(
			type: "OutOfBoundsTile",
			name: "out of bounds",
			symbol: ' ',
			fg: "black",
			bg: "black",
			slope: 0,
			mutable: false
		);

	}
}
