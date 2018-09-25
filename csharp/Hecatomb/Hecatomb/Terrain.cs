/*
 * Created by SharpDevelop.
 * User: m543015
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
	public class Terrain
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
			string name,
			string fg = "white",
			string bg = "black",
			char symbol = ' ',
			bool opaque = false,
			bool solid = false,
			bool fallable = false,
			bool mutable = true,
			int zview = 0,
			int zwalk = 0
		)
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
			name: "boundary",
			opaque: true,
			solid: true,
			mutable: false
		);
		
		public static readonly Terrain EmptyTile = new Terrain(
			name: "empty",
			symbol: '.',
			fg: "dark gray",
			fallable: true,
			zview: -1
		);
		
		public static readonly Terrain FloorTile = new Terrain(
			name: "floor",
			symbol: '.'
		);
		
		public static readonly Terrain WallTile = new Terrain(
			name: "wall",
			symbol: '#',
			opaque: true,
			solid: true
		);
		
		public static readonly Terrain UpSlopeTile = new Terrain(
			name: "upward slope",
			symbol: '\u02C4',
			zview: +1,
			zwalk: +1
		);
		
		public static readonly Terrain DownSlopeTile = new Terrain(
			name: "downward slope",
			symbol: '\u02C5',
			zview: -1,
			zwalk: -1
		);	
	}
}
