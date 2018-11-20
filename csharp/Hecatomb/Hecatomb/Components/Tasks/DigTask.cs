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
			MenuName = "dig or harvest";
			TypeName = "dig";
		}
			
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			f.Symbol = '\u2717';
			f.FG = "white";
		}
		public override void Finish()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "DigTaskComplete" });
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			Game.World.Features[x, y, z].Despawn();
			var tiles = Game.World.Tiles;
            var covers = Game.World.Covers;
			Terrain t = tiles[x, y, z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
            Cover none = Cover.NoCover;
			if (t==floor)
			{
				Terrain tb = Game.World.GetTile(x, y, z-1);
				if (tb==wall)
				{
					tiles[x, y, z] = down;
					tiles[x, y, z-1] = up;
                    covers[x, y, z] = none;
                    covers[x, y, z - 1].Mine(x, y, z - 1);
                    covers[x, y, z-1] = none;
                } else if (tb==up)
				{
					tiles[x, y, z] = down;
                    covers[x, y, z] = none;
                }
				else if (tb==empty || tb==down || tb==floor)
				{
					tiles[x, y, z] = empty;
                    covers[x, y, z] = none;
                }
			}
			else if (t==up)
			{
				tiles[x, y, z] = floor;
                covers[x, y, z] = none;
                tiles[x, y, z + 1] = empty;
                covers[x, y, z + 1] = none;
            }
			else if (t==down)
			{
				tiles[x, y, z] = empty;
				tiles[x, y, z-1] = floor;
                covers[x, y, z] = none;
                covers[x, y, z-1] = none;
            }
			else if (t==wall)
			{
				tiles[x, y, z] = floor;
                covers[x, y, z].Mine(x, y, z);
                covers[x, y, z] = none;
            }		
			base.Finish();
		}
		
		public override void ChooseFromMenu()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseDigTask" });
//			Game.Controls.Set(new SelectTileControls(this));
			Game.Controls.Set(new SelectZoneControls(this));
		}
		
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (!Game.World.Explored.Contains(c))
            {
                co.MenuMiddle = new List<string>() {"Unexplored tile."};
                co.MiddleColors[0, 0] = "orange";
            }
            else if (ValidTile(c))
            {
                co.MenuMiddle = new List<string>() { String.Format("Dig or harvest from {0} {1} {2}.", c.X, c.Y, c.Z) };
                co.MiddleColors[0, 0] = "green";
            }
            else
            {
                co.MenuMiddle = new List<string>() {"Can't dig or harvest here."};
                co.MiddleColors[0, 0] = "orange";
            }
		}
		public override void TileHover(Coord c, List<Coord> squares)
		{
			var co = Game.Controls;
			co.MenuMiddle.Clear();
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Tiles[x, y, z];
                if (Game.World.Explored.Contains(square))
                {
                    if (Game.World.Features[x, y, z]?.TryComponent<Harvestable>()!=null)
                    {
                        priority = 5;
                    }
                    else if (t == Terrain.WallTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 4);
                    }
                    else if (t == Terrain.FloorTile)
                    {
                        priority = Math.Max(priority, 3);
                    }
                    else if (t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 2);
                    }
                }
                else
                {
                    priority = Math.Max(priority, 1);
                }
            }
            string color = "green";
            string txt;
            if (priority==5)
            {
                txt = "Harvest area.";
            }
            else if (priority==4)
            {
                txt = "Dig corridors and remove slopes in this area.";
            }
            else if (priority==3)
            {
                txt = "Dig through floors in this area.";
            }
            else if (priority==2)
            {
                txt = "Remove slopes below this area.";
            }
            else if (priority==1)
            {
                txt = "Dig or harvest unexplored area.";
                color = "orange";
            }
            else
            {
                txt = "Cannot dig or harvest area.";
                color = "orange";
            } 
			co.MenuMiddle = new List<string>() {txt};
            co.MiddleColors[0, 0] = color;
        }
        public override void SelectZone(List<Coord> squares)
        {
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Tiles[x, y, z];
                if (Game.World.Explored.Contains(square))
                {
                    if (Game.World.Features[x, y, z]?.TryComponent<Harvestable>() != null)
                    {
                        priority = 5;
                    }
                    else if (t == Terrain.WallTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 4);
                    }
                    else if (t == Terrain.FloorTile)
                    {
                        priority = Math.Max(priority, 3);
                    }
                    else if (t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 2);
                    }
                }
                else
                {
                    priority = Math.Max(priority, 1);
                }
            }
            foreach (Coord square in squares)
            {
                if (Game.World.Tasks[square.X, square.Y, square.Z] != null)
                {
                    continue;
                }
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Tiles[x, y, z];
                if (priority == 5 && Game.World.Features[x, y, z]?.TryComponent<Harvestable>() != null)
                {
                    Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateHarvestTask" });
                    Game.World.Entities.Spawn<TaskEntity>("HarvestTask").Place(x, y, z);
                } else if (    (priority == 4 && (t == Terrain.WallTile || t == Terrain.UpSlopeTile))
                        || (priority == 3 && t == Terrain.FloorTile)
                        || (priority == 2 && t == Terrain.DownSlopeTile)
                        || !Game.World.Explored.Contains(square))
               {
                    // should I cancel existing tasks?
                    if (Game.World.Tasks[x, y, z] == null)
                        Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateDigTask" });
                        Game.World.Entities.Spawn<TaskEntity>("DigTask").Place(x, y, z);
                    }
               }
            }

        public override bool ValidTile(Coord c)
        {
            // what about non-harvestable, i.e. owned features?
            // maybe make those the very lowest priority?
            // in order to avoid giving away unexplored terrain, always allow designation
            if (!Game.World.Explored.Contains(c))
            {
                return true;
            }
            Terrain t = Game.World.Tiles[c.X, c.Y, c.Z];
            Terrain tb = Game.World.Tiles[c.X, c.Y, c.Z - 1];
            // can't dig an empty tile no matter what
            if (t==Terrain.EmptyTile)
            {
                return false;
            }
            // can't dig through the bottom of the level
            if (t==Terrain.FloorTile && tb==Terrain.VoidTile)
            {
                return false;
            }
            // there's a whole song and dance about hardness, but let's hold off on that
            if (Game.World.Covers[c.X, c.Y, c.Z].Hardness>0)
            {
                return false;
            }
            return true;
        }
	}
}
