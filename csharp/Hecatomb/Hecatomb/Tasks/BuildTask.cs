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
    using static HecatombAliases;
	public class BuildTask : Task
	{
		public BuildTask(): base()
		{
			MenuName = "build floors or walls";
			Ingredients = new Dictionary<string, int>
			{
				{"Rock", 1}
			};
		}

        public override string GetDisplayName()
        {
            var tiles = Game.World.Terrains;
            Terrain t = tiles[X, Y, Z];
            Terrain floor = Terrain.FloorTile;
            Terrain wall = Terrain.WallTile;
            Terrain up = Terrain.UpSlopeTile;
            Terrain down = Terrain.DownSlopeTile;
            Terrain empty = Terrain.EmptyTile;
            if (t == floor || t == up)
            {
                return "build wall";
            }
            return "build floor";
        }
        public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[X, Y, Z];
			f.Symbol = '\u2692';
			f.FG = "white";
		}
		public override void Finish()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "BuildTaskComplete" });
			Game.World.Features[X, Y, Z].Despawn();
			var tiles = Game.World.Terrains;
			Terrain t = tiles[X, Y, Z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
			if (t==floor || t==up)
			{
				tiles[X, Y, Z] = wall;
                Game.World.Covers[X, Y, Z] = Cover.Soil;
				Terrain ta = Game.World.GetTile(X, Y, Z+1);
				if (ta==empty || ta==down)
				{
					tiles[X, Y, Z+1] = floor;
				}
			}
			else if (t==empty || t==down)
			{
				tiles[X, Y, Z] = floor;
			}
			base.Finish();
            Game.World.ValidateOutdoors();
		}

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (ValidTile(c))
            {
                co.MenuMiddle = new List<ColoredText>() { "{green}"+String.Format("Build walls or floors from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange}Can't build here." };
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
                Terrain t = Game.World.Terrains[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    if (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 2);
                    }
                    else if (t == Terrain.FloorTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 1);
                    }
                }
            }
            string txt;
            if (priority == 2)
            {
                txt = "{green}Build floors in this area.";
            }
            else if (priority == 1)
            {
                txt = "{green}Build walls in this area.";
            }
            else
            {
                txt = "{orange}Cannot build in this area.";
            }
            co.MenuMiddle = new List<ColoredText>() { txt };
        }
        public override void SelectZone(List<Coord> squares)
        {
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Terrains[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    if (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 2);
                    }
                    else if (t == Terrain.FloorTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 1);
                    }
                }
            }
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Terrains[x, y, z];
                if ((priority == 2 && (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile))
                      || (priority == 1 && t == Terrain.FloorTile || t == Terrain.UpSlopeTile))
                {
                    // should I cancel existing tasks?
                    if (Game.World.Tasks[x, y, z] == null && Game.World.Features[x, y, z]==null)
                        Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateBuildTask" });
                    Entity.Spawn<BuildTask>().Place(x, y, z);
                }
            }
        }


        public override void ChooseFromMenu()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseBuildTask" });
            Game.Controls.Set(new SelectZoneControls(this));
		}

        public override bool ValidTile(Coord c)
        {
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            // can't build on a wall tile
            if (Game.World.Terrains[c.X, c.Y, c.Z] == Terrain.WallTile)
            {
                return false;
            }
            // can't build on a feature
            else if (Game.World.Features[c.X, c.Y, c.Z]!=null)
            {
                return false;
            }
            return true;
        }
    }
}
