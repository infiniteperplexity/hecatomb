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
	public class Species : FlyWeight<Species>
	{
		public readonly string Name;
		public readonly char Symbol;
		public readonly string FG;
		public readonly string BG;

		public Species(
			string type = "",
			string name = "",
			string fg = "white",
			string bg = "black",
			char symbol = ' '
		) : base(type)
		{
			Name = name;
			FG = fg;
			BG = bg;
			Symbol = symbol;
		}

		public static readonly Species Human = new Species(
			type: "Human",
			name: "human",
			symbol: '@'
		);

		public static readonly Species Spider = new Species(
			type: "Spider",
			name: "spider",
			symbol: 's'
		);

		public static readonly Species Dryad = new Species(
			type: "Dryad",
			name: "dryad",
			symbol: 'n'
		);
	}
}
