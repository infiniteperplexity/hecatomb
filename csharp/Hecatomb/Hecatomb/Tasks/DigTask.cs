﻿/*
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
    using static HecatombAliases;
	public class DigTask : Task
	{
		public DigTask(): base()
		{
			MenuName = "dig, harvest, or deconstruct";
            Makes = "Excavation";
            BG = "orange";
		}

        public override string GetDisplayName()
        {
            var tiles = Game.World.Terrains;
            var covers = Game.World.Covers;
            Terrain t = tiles[X, Y, Z];
            Terrain floor = Terrain.FloorTile;
            Terrain wall = Terrain.WallTile;
            Terrain up = Terrain.UpSlopeTile;
            Terrain down = Terrain.DownSlopeTile;
            Terrain empty = Terrain.EmptyTile;
            Cover none = Cover.NoCover;
            if (t == floor)
            {
                return "dig pit";
            }
            else if (t == up)
            {
                return "dig away slope";
            }
            else if (t == down)
            {
                return "dig away slope";
            }
            return "dig corridor";
        }
			
		public override void Start()
		{
			base.Start();
			Feature f = Game.World.Features[X, Y, Z];
            f.Name = "incomplete excavation";
            f.GetComponent<IncompleteFixtureComponent>().Makes = "Excavation";
			f.Symbol = '\u2717';
			f.FG = "white";
		}
		public override void Finish()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "DigTaskComplete" });
			Game.World.Features[X, Y, Z].Despawn();
			var tiles = Game.World.Terrains;
            var covers = Game.World.Covers;
			Terrain t = tiles[X, Y, Z];
            Terrain tb = Game.World.GetTile(X, Y, Z - 1);
            Terrain floor = Terrain.FloorTile;
			Terrain wall = Terrain.WallTile;
			Terrain up = Terrain.UpSlopeTile;
			Terrain down = Terrain.DownSlopeTile;
			Terrain empty = Terrain.EmptyTile;
            Cover none = Cover.NoCover;
            string evnt = "None";
			if (t==floor)
			{
                // dig a pit
				if (tb==wall)
				{
					tiles[X, Y, Z] = down;
					tiles[X, Y, Z-1] = up;
                    covers[X, Y, Z] = none;
                    covers[X, Y, Z - 1].Mine(X, Y, Z - 1);
                    Cover.ClearCover(X, Y, Z - 1);
                    evnt = "Pit";
                    
                } else if (tb==up)
				{
                // dig a hole in the floor
                    tiles[X, Y, Z] = down;
                    covers[X, Y, Z] = none;
                    evnt = "Hole";
                    
                }
                // dig a hole in the floor
				else if (tb==empty || tb==down || tb==floor)
				{
					tiles[X, Y, Z] = empty;
                    covers[X, Y, Z] = none;
                    evnt = "Hole";
                }
                foreach (Coord c in Tiles.GetNeighbors8(X, Y, Z - 1))
                {
                    // only add the minerals below?
                    if (covers[c.X, c.Y, c.Z].Mineral != null)
                    {
                        Explored.Add(c);
                    }
                }
                foreach (Coord c in Tiles.GetNeighbors10(X, Y, Z))
                {
                    // only add the minerals below?
                    if (covers[c.X, c.Y, c.Z].Mineral != null || c.Z >= Z)
                    {
                        Explored.Add(c);
                    }
                }
            }
			else if (t==up)
			{
                // level a slope
				tiles[X, Y, Z] = floor;
                covers[X, Y, Z] = none;
                tiles[X, Y, Z + 1] = empty;
                Cover.ClearCover(X, Y, Z + 1);
                evnt = "Level";
                foreach (Coord c in Tiles.GetNeighbors10(X, Y, Z))
                {
                    Explored.Add(c);
                }
            }
			else if (t==down)
			{
                // level the bottom of a pit
				tiles[X, Y, Z] = empty;
				tiles[X, Y, Z-1] = floor;
                covers[X, Y, Z] = none;
                covers[X, Y, Z-1] = none;
                evnt = "LevelPit";
                foreach (Coord c in Tiles.GetNeighbors10(X, Y, Z-1))
                {
                    Explored.Add(c);
                }
            }
			else if (t==wall)
			{
                // dig a corridor
                tiles[X, Y, Z] = floor;
                covers[X, Y, Z].Mine(X, Y, Z);
                covers[X, Y, Z] = none;
                evnt = "Corridor";
                foreach (Coord c in Tiles.GetNeighbors10(X, Y, Z))
                {
                    Explored.Add(c);
                }
            }
            Game.World.Events.Publish(new DigEvent() { X = X, Y = Y, Z = Z, EventType = evnt});
            base.Finish();
            Game.World.ValidateOutdoors();
		}
		
		public override void ChooseFromMenu()
		{
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseDigTask" });
            var c = new SelectZoneControls(this);
            c.MenuSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            ControlContext.Set(c);
        }
		
		public override void TileHover(Coord c)
		{
			var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                co.MenuMiddle = new List<ColoredText>() {"{yellow}Unexplored tile."};
            }
            else if (ValidTile(c))
            {
                co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Dig, harvest, or deconstruct from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() {"{orange}Can't dig, harvest, or deconstruct here."};
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
                Feature f = Game.World.Features[x, y, z];
                Task ta = Game.World.Tasks[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    // right now, incomplete dig tasks are the absolute top priority.  is that really what we want? or should they count the same as the terrain they're on?
                    if (f?.TypeName == "IncompleteFeature" && ta == null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                    {
                        priority = 7;
                    }
                    else if (f?.TypeName == "IncompleteFeature" || f?.TryComponent<Fixture>() != null || f?.TryComponent<StructuralComponent>() != null)
                    {
                        priority = Math.Max(priority, 2);
                    }
                    else if (f != null && f.TryComponent<Harvestable>()!=null && f.TryComponent<StructuralComponent>()==null)
                    {
                        priority = 6;
                    }
                    else if (t == Terrain.WallTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 5);
                    }
                    else if (t == Terrain.FloorTile)
                    {
                        priority = Math.Max(priority, 4);
                    }
                    else if (t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 3);
                    }        
                }
                else
                {
                    priority = Math.Max(priority, 1);
                }
            }
            string txt;
            if (priority == 7)
            {
                txt = "{green}Finish dig task.";
            }
            else if (priority == 6)
            {
                txt = "{green}Harvest area.";
            }
            else if (priority == 5)
            {
                txt = "{green}Dig corridors and remove slopes in this area.";
            }
            else if (priority == 4)
            {
                txt = "{green}Dig through floors in this area.";
            }
            else if (priority == 3)
            {
                txt = "{green}Remove slopes below this area.";
            }
            else if (priority == 2)
            {
                txt = "{orange}Remove fixtures and structures in this area.";
            }
            else if (priority == 1)
            {
                txt = "{orange}Dig unexplored area.";
            }
            else
            {
                txt = "{orange}Cannot dig, harvest, or deconstruct area.";
            } 
			co.MenuMiddle = new List<ColoredText>() {txt};
        }
        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "DigTask", squares: squares);
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Game.World.Terrains[x, y, z];
                Feature f = Game.World.Features[x, y, z];
                Task ta = Game.World.Tasks[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    // right now, incomplete dig tasks are the absolute top priority.  is that really what we want? or should they count the same as the terrain they're on?
                    if (f?.TypeName == "IncompleteFeature" && ta == null && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                    {
                        priority = 7;
                    }
                    if (f != null && f.TryComponent<Harvestable>() != null && f.TryComponent<StructuralComponent>() == null)
                    {
                        priority = 6;
                    }
                    else if (f?.TypeName == "IncompleteFeature" || f?.TryComponent<Fixture>() != null || f?.TryComponent<StructuralComponent>() != null)
                    {
                        priority = Math.Max(priority, 2);
                    }
                    else if (t == Terrain.WallTile || t == Terrain.UpSlopeTile)
                    {
                        priority = Math.Max(priority, 5);
                    }
                    else if (t == Terrain.FloorTile)
                    {
                        priority = Math.Max(priority, 4);
                    }
                    else if (t == Terrain.DownSlopeTile)
                    {
                        priority = Math.Max(priority, 3);
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
                Terrain t = Game.World.Terrains[x, y, z];
                Feature f = Game.World.Features[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    // complete existing dig task
                    if (priority == 7 && f?.TypeName == "IncompleteFeature" && f.GetComponent<IncompleteFixtureComponent>().Makes == "Excavation")
                    {
                        Entity.Spawn<DigTask>().Place(x, y, z);
                    }
                    else if (priority == 6 && Game.World.Features[x, y, z]?.TryComponent<Harvestable>() != null && Game.World.Features[x, y, z]?.TryComponent<StructuralComponent>()==null)
                    {
                        Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateHarvestTask" });
                        Entity.Spawn<HarvestTask>().Place(x, y, z);
                    }
                    else if (priority == 2 && ((f?.TryComponent<IncompleteFixtureComponent>() != null && f?.GetComponent<IncompleteFixtureComponent>().Makes != "Excavation") || f?.TryComponent<Fixture>() != null || f?.TryComponent<StructuralComponent>() != null))
                    {
                        // this arguably needs to be a differently named task, although I think this currently does the job
                        Entity.Spawn<HarvestTask>().Place(x, y, z);
                    }
                    else if ((priority == 5 && (t == Terrain.WallTile || t == Terrain.UpSlopeTile))
                         || (priority == 4 && t == Terrain.FloorTile)
                         || (priority == 3 && t == Terrain.DownSlopeTile))
                    {
                        if (f == null)
                        {
                            // should I cancel existing tasks?
                            if (Game.World.Tasks[x, y, z] == null)
                                Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateDigTask" });
                            Entity.Spawn<DigTask>().Place(x, y, z);
                        }
                    }
                }
                else if (priority == 1 || priority == 5)
                {
                    // should I cancel existing tasks?
                    if (Game.World.Tasks[x, y, z] == null)
                        Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateDigTask" });
                    Entity.Spawn<DigTask>().Place(x, y, z);
                }
            }
        }

        public override bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            // what about non-harvestable, i.e. owned features?
            // maybe make those the very lowest priority?
            // in order to avoid giving away unexplored terrain, always allow designation
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                return true;
            }
            Terrain t = Game.World.Terrains[x, y, z];
            Terrain tb = Game.World.Terrains[x, y, z];
            // can't dig an empty tile no matter what
            if (t==Terrain.EmptyTile)
            {
                return false;
            }
            // I think this is the correct priority for harvesting
            // oh wait, I made this a separate task...urgh...
            Feature f = Game.World.Features[x, y, z];
            if (f!=null)
            {
                if (f.TryComponent<Harvestable>()!=null)
                {
                    return true;
                }
                else if (f.TypeName=="IncompleteFeature")
                {
                    return true;
                }
                else if (f.TryComponent<Fixture>() != null)
                {
                    return true;
                }
                else if (f.TryComponent<StructuralComponent>() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // can't dig through the bottom of the level
            if (t==Terrain.FloorTile && tb==Terrain.VoidTile)
            {
                return false;
            }
            // **** We need a more sophisticated hardness check

            if (TooHard(c))
            {
                return false;
            }
            return true;
        }


        public bool TooHard(Coord c)
        {
            Terrain tn = Game.World.Terrains[c.X, c.Y, c.Z];
            int hardness;
            if (tn == Terrain.WallTile || tn == Terrain.UpSlopeTile)
            {
                hardness = Game.World.Covers[c.X, c.Y, c.Z].Hardness;
            }
            else
            {
                hardness = Game.World.Covers[c.X, c.Y, c.Z - 1].Hardness;
            }
            if (Game.Options.IgnoreHardness || Game.World.GetState<ResearchHandler>().GetToolHardness() >= hardness)
            {
                return false;
            }
            return true;
        }

        public override bool CanAssign(Creature c)
        {
            Coord crd = new Coord(X, Y, Z);
            if (!Explored.Contains(crd) && !Options.Explored)
            {
                return false;
            }
            if (!Placed)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                if (TooHard(crd))
                {
                    Status.PushMessage("Canceling dig task; material too hard.");
                }
                else
                {
                    Status.PushMessage("Canceling invalid dig task.");
                }
                Cancel();
                return false;
            }
            // ValidTile produces misleading results if a harvest or deconstruct task could have been placed
            else if(Features[X, Y, Z] != null)
            {
                Feature f = Features[X, Y, Z];
                if (f.TryComponent<IncompleteFixtureComponent>() == null || f.GetComponent<IncompleteFixtureComponent>().Makes != "Excavation")
                {
                    Status.PushMessage("Canceling invalid dig  task.");
                    Cancel();
                    return false;
                }
            }
            Movement m = c.GetComponent<Movement>();
            return m.CanReach(this, useLast: (WorkRange == 0)) && m.CanFindResources(Ingredients);
        }
    }
}
