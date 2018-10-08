/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	public class DigTask : Task
	{
		public DigTask(): base()
		{
			MenuName = "dig corridors/pits/slopes";
		}
			
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[Entity.x, Entity.y, Entity.z];
			f.Symbol = '\u2717';
			f.FG = "white";
		}
		public override void Finish()
		{
			int x = Entity.x;
			int y = Entity.y;
			int z = Entity.z;
			Game.World.Features[x, y, z].Remove();
			var tiles = Game.World.Tiles;
			Terrain t = tiles[x, y, z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
			if (t==floor)
			{
				Terrain tb = Game.World.GetTile(x, y, z-1);
				if (tb==wall)
				{
					tiles[x, y, z] = down;
					tiles[x, y, z-1] = up;
				} else if (tb==up)
				{
					tiles[x, y, z] = down;
				}
				else if (tb==empty || tb==down || tb==floor)
				{
					tiles[x, y, z] = empty;
				}
			}
			else if (t==up)
			{
				tiles[x, y, z] = floor;
			}
			else if (t==down)
			{
				tiles[x, y, z] = empty;
				tiles[x, y, z-1] = floor;
			}
			else if (t==wall)
			{
				tiles[x, y, z] = down;
				tiles[x, y, z-1] = up;
			}		
			Complete();
		}
		
		public override void ChooseFromMenu()
		{
//			Game.Controls.Set(new SelectTileControls(this));
			Game.Controls.Set(new SelectZoneControls(this));
		}
		
		public override void TileHover(Coord c)
		{
			Debug.Print("Dig out tiles starting at {0} {1} {2}", c.x, c.y, c.z);
		}
		public override void TileHover(Coord c, List<Coord> squares)
		{
			Debug.Print("Dig out tiles finishing at {0} {1} {2}", c.x, c.y, c.z);
		}
	}

}
