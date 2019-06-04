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
    using static HecatombAliases;
	public class DigTask : Task
	{
		public DigTask(): base()
		{
			MenuName = "dig, harvest, or dismantle";
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
                    covers[X, Y, Z-1] = none;
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
                    Explored.Add(c);
                }
                foreach (Coord c in Tiles.GetNeighbors10(X, Y, Z))
                {
                    Explored.Add(c);
                }
            }
			else if (t==up)
			{
                // level a slope
				tiles[X, Y, Z] = floor;
                covers[X, Y, Z] = none;
                tiles[X, Y, Z + 1] = empty;
                covers[X, Y, Z + 1] = none;
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
			Game.Controls.Set(new SelectZoneControls(this));
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
                co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Dig, harvest, or dismantle from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() {"{orange}Can't dig, harvest, or dismantle here."};
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
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    if (f?.TryComponent<Harvestable>()!=null)
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
            string txt;
            if (priority==6)
            {
                txt = "{green}Harvest area.";
            }
            else if (priority==5)
            {
                txt = "{green}Dig corridors and remove slopes in this area.";
            }
            else if (priority==4)
            {
                txt = "{green}Dig through floors in this area.";
            }
            else if (priority==3)
            {
                txt = "{green}Remove slopes below this area.";
            }
            else if (priority==2)
            {
                txt = "{orange}Remove fixtures and structures in this area.";
            }
            else if (priority == 1)
            {
                txt = "{orange}Dig unexplored area.";
            }
            else
            {
                txt = "{orange}Cannot dig, harvest, or dismantle area.";
            } 
			co.MenuMiddle = new List<ColoredText>() {txt};
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
                Feature f = Game.World.Features[x, y, z];
                if (Game.World.Explored.Contains(square) || Options.Explored)
                {
                    if (f?.TryComponent<Harvestable>() != null)
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
                if (priority == 6 && Game.World.Features[x, y, z]?.TryComponent<Harvestable>() != null)
                {
                    Game.World.Events.Publish(new TutorialEvent() { Action = "DesignateHarvestTask" });
                    Entity.Spawn<HarvestTask>().Place(x, y, z);
                }
                else if (priority == 2 && (f?.TypeName=="IncompleteFeature" || f?.TryComponent<Fixture>() != null || f?.TryComponent<StructuralComponent>() != null))
                {
                    // this arguably needs to be a differently named task, although I think this currently does the job
                    Entity.Spawn<HarvestTask>().Place(x, y, z);
                }
                else if ((priority == 5 && (t == Terrain.WallTile || t == Terrain.UpSlopeTile))
                     || (priority == 4 && t == Terrain.FloorTile)
                     || (priority == 3 && t == Terrain.DownSlopeTile)
                     || (!Game.World.Explored.Contains(square) && !Options.Explored))
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
            // there's a whole song and dance about hardness, but let's hold off on that
            if (Game.Options.IgnoreHardness)
            {
                return true;
            }
            int hardness = Game.World.Covers[c.X, c.Y, c.Z].Hardness;
            if (hardness>0 && !Game.World.GetState<ResearchHandler>().Researched.Contains("FlintTools"))
            {
                return false;
            }
            return true;
        }
	}
}
