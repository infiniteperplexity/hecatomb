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
            Makes = "Construction";
			Ingredients = new Dictionary<string, int>
			{
				{"Rock", 1}
			};
            BG = "purple";
		}

        public override string GetDisplayName()
        {
            var tiles = OldGame.World.Terrains;
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
			Feature f = OldGame.World.Features[X, Y, Z];
            f.Name = "incomplete construction";
            f.GetComponent<IncompleteFixtureComponent>().Makes = "Construction";
            f.Symbol = '\u2692';
			f.FG = "white";
		}
		public override void Finish()
		{
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "BuildTaskComplete" });
			OldGame.World.Features[X, Y, Z].Despawn();
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "AnyBuildComplete" });
            var tiles = OldGame.World.Terrains;
			Terrain t = tiles[X, Y, Z];	
			Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
			if (t==floor || t==up)
			{
				tiles[X, Y, Z] = wall;
                OldGame.World.Covers[X, Y, Z] = Cover.Soil;
				Terrain ta = OldGame.World.GetTile(X, Y, Z+1);
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
            OldGame.World.ValidateOutdoors();
		}

        public override void TileHover(Coord c)
        {
            var co = OldGame.Controls;
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
            var co = OldGame.Controls;
            co.MenuMiddle.Clear();
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = OldGame.World.Terrains[x, y, z];
                Feature f = OldGame.World.Features[x, y, z];
                if (OldGame.World.Explored.Contains(square) || Options.Explored)
                {
                    if (f != null && f.TryComponent<IncompleteFixtureComponent>()!=null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                    {
                        priority = Math.Max(priority, 3);
                    }
                    else if (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile)
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
            if (priority == 3)
            {
                txt = "{green}Undo digging in this area.";
            }
            else if (priority == 2)
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
            CommandLogger.LogCommand(command: "BuildTask", squares: squares);
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = OldGame.World.Terrains[x, y, z];
                Feature f = OldGame.World.Features[x, y, z];
                if (OldGame.World.Explored.Contains(square) || Options.Explored)
                {
                    if (f != null && f.TryComponent<IncompleteFixtureComponent>() != null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                    {
                        priority = Math.Max(priority, 3);
                    }
                    else if (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile)
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
                if (OldGame.World.Tasks[x, y, z] != null)
                {
                    continue;
                }
                Terrain t = OldGame.World.Terrains[x, y, z];
                Feature f = OldGame.World.Features[x, y, z];
                if (f?.TryComponent<IncompleteFixtureComponent>() != null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Construction")
                {
                    Entity.Spawn<BuildTask>().Place(x, y, z);
                }
                // filling in a prior excavation
                else if (priority == 3 && f?.TryComponent<IncompleteFixtureComponent>() != null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                {
                    HarvestTask ht = Entity.Spawn<HarvestTask>();
                    ht.Place(x, y, z);
                    ht.Ingredients = new Dictionary<string, int>() { { "Rock", 1 } };
                }
                else if ((priority == 2 && (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile))
                      || (priority == 1 && t == Terrain.FloorTile || t == Terrain.UpSlopeTile))
                {
                    // should I cancel existing tasks?
                    if (OldGame.World.Tasks[x, y, z] == null && OldGame.World.Features[x, y, z]==null)
                        OldGame.World.Events.Publish(new TutorialEvent() { Action = "DesignateBuildTask" });
                    Entity.Spawn<BuildTask>().Place(x, y, z);
                }
            }
        }


        public override void ChooseFromMenu()
		{
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ChooseBuildTask" });
            var c = new SelectZoneControls(this);
            c.MenuSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            ControlContext.Set(c);
		}

        public override bool ValidTile(Coord c)
        {
            Feature f = OldGame.World.Features[c.X, c.Y, c.Z];
            if (!OldGame.World.Explored.Contains(c) && !Options.Explored)
            {
                return false;
            }
            // can't build on most features
            if (f != null)
            {
                if (f.TryComponent<IncompleteFixtureComponent>() != null && (f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation" || f.GetComponent<IncompleteFixtureComponent>().Makes == "Construction"))
                {
                    return true;
                }
                return false;
            }
            // can't build on a wall tile
            if (OldGame.World.Terrains[c.X, c.Y, c.Z] == Terrain.WallTile)
            {
                return false;
            }
            return true;
        }
    }
}
