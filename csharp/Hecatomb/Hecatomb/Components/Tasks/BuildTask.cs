/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 9:04 AM
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
// should this just do an outline?

namespace Hecatomb
{
	public class BuildTask : Task
	{
		public BuildTask(): base()
		{
			MenuName = "build floors or walls";
			TypeName = "build";
			Ingredients = new Dictionary<string, int>
			{
				{"Rock", 1}
			};
		}
			
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			f.Symbol = '\u2692';
			f.FG = "white";
		}
		public override void Finish()
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			Game.World.Features[x, y, z].Remove();
			var tiles = Game.World.Tiles;
			Terrain t = tiles[x, y, z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
			if (t==floor || t==up)
			{
				tiles[x, y, z] = wall;
				Terrain ta = Game.World.GetTile(x, y, z+1);
				if (ta==empty || ta==down)
				{
					tiles[x, y, z+1] = floor;
				}
			}
			else if (t==empty || t==down)
			{
				tiles[x, y, z] = floor;
			}
			base.Finish();
		}
		
		public override void ChooseFromMenu()
		{
			Game.Controls.Set(new SelectZoneControls(this));
		}
		
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<string>() {String.Format("Build from {0} {1} {2}", c.X, c.Y, c.Z)};
			co.MiddleColors[0,0] = "green";
		}
		public override void TileHover(Coord c, List<Coord> squares)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
			co.MenuMiddle = new List<string>() {String.Format("Build to {0} {1} {2}", c.X, c.Y, c.Z)};
			co.MiddleColors[0,0] = "red";
		}
	}
}
