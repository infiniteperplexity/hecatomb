using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Excavation : Feature
    {
        public Excavation()
        {
            _name = "excavation";
            _fg = "white";
            _symbol = '\u2717';
        }
    }
    public class DigTask : Task
    {
        public DigTask() : base()
        {
            _name = "dig, harvest, or deconstruct";
            Makes = typeof(Excavation);
            _bg = "orange";
        }

        protected override string? getName()
        {
            if (!Placed)
            {
                return _name;
            }
            var tiles = Terrains;
            var covers = Covers;
            Terrain t = tiles.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
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
            if (!Placed)
            {
                return;
            }
            base.Start();
            Feature? f = Features.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
            if (f is IncompleteFixture)
            {
                var ifx = (IncompleteFixture)f;
                ifx.Makes = Makes;
                ifx.IncompleteSymbol = '\u2717';
                ifx.IncompleteFG = "white";
            }
        }
        public override void Finish()
        {
            if (!Placed)
            {
                Despawn();
            }
            var (x, y, z) = GetPlacedCoordinate();
            Publish(new TutorialEvent() { Action = "DigTaskComplete" });
            Features.GetWithBoundsChecked(x, y, z)?.Despawn();
            Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
            Terrain tb = Terrains.GetWithBoundsChecked(x, y, z - 1);
            Terrain floor = Terrain.FloorTile;
            Terrain wall = Terrain.WallTile;
            Terrain up = Terrain.UpSlopeTile;
            Terrain down = Terrain.DownSlopeTile;
            Terrain empty = Terrain.EmptyTile;
            Cover none = Cover.NoCover;
            string evnt = "None";
            if (t == floor)
            {
                // dig a pit
                if (tb == wall)
                {
                    Terrains.SetWithBoundsChecked(x, y, z, down);
                    Terrains.SetWithBoundsChecked(x, y, z - 1, up);
                    Cover.ClearGroundCover(x, y, z);
                    Cover.Mine(x, y, z - 1);
                    evnt = "Pit";

                }
                else if (tb == up)
                {
                    // dig a hole in the floor
                    Terrains.SetWithBoundsChecked(x, y, z, down);
                    Cover.ClearGroundCover(x, y, z);
                    evnt = "Hole";

                }
                // dig a hole in the floor
                else if (tb == empty || tb == down || tb == floor)
                {
                    Terrains.SetWithBoundsChecked(x, y, z, empty);
                    Cover.ClearGroundCover(x, y, z);
                    evnt = "Hole";
                }
                foreach (Coord c in Tiles.GetNeighbors8(x, y, z - 1))
                {
                    // only add the minerals below?
                    if (Covers.GetWithBoundsChecked(c.X, c.Y, c.Z).Resource != null)
                    {
                        Explored.Add(c);
                    }
                }
                foreach (Coord c in Tiles.GetNeighbors10(x, y, z))
                {
                    // only add the minerals below?
                    if (Covers.GetWithBoundsChecked(c.X, c.Y, c.Z).Resource != null || c.Z >= Z)
                    {
                        Explored.Add(c);
                    }
                }
            }
            else if (t == up)
            {
                // level a slope
                Terrains.SetWithBoundsChecked(x, y, z, floor);
                Cover.ClearGroundCover(x, y, z);
                Terrains.SetWithBoundsChecked(x, y, z + 1, empty);
                Cover.ClearGroundCover(x, y, z + 1);
                evnt = "Level";
                foreach (Coord c in Tiles.GetNeighbors10(x, y, z))
                {
                    Explored.Add(c);
                }
            }
            else if (t == down)
            {
                // level the bottom of a pit
                Terrains.SetWithBoundsChecked(x, y, z, empty);
                Cover.ClearGroundCover(x, y, z);
                Terrains.SetWithBoundsChecked(x, y, z - 1, floor);
                Cover.ClearGroundCover(x, y, z - 1);
                evnt = "LevelPit";
                foreach (Coord c in Tiles.GetNeighbors10(x, y, z - 1))
                {
                    Explored.Add(c);
                }
            }
            else if (t == wall)
            {
                // dig a corridor
                Terrains.SetWithBoundsChecked(x, y, z, floor);
                Cover.Mine(x, y, z);
                evnt = "Corridor";
                foreach (Coord c in Tiles.GetNeighbors10(x, y, z))
                {
                    Explored.Add(c);
                }
            }
            Publish(new DigEvent() { X = x, Y = y, Z = z, EventType = evnt });
            base.Finish();
            GameState.World!.ValidateOutdoors();
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseDigTask" });
            var c = new SelectZoneControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Dig, harvest, or deconstruct." };
            InterfaceState.SetControls(c);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                co.InfoMiddle = new List<ColoredText>() { "{yellow}Unexplored tile." };
            }
            else if (ValidTile(c))
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Dig, harvest, or deconstruct from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Can't dig, harvest, or deconstruct here." };
            }
        }
        public override void TileHover(Coord c, List<Coord> squares)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            int priority = 0;
            foreach (Coord square in squares)
            {
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
                Feature? f = Features.GetWithBoundsChecked(x, y, z);
                Task? ta = Tasks.GetWithBoundsChecked(x, y, z);
                if (Explored.Contains(square) || HecatombOptions.Explored)
                {
                    // right now, incomplete dig tasks are the absolute top priority.  is that really what we want? or should they count the same as the terrain they're on?
                    if (f is IncompleteFixture && ta is null && (f as IncompleteFixture)!.Makes == typeof(Excavation))
                    {
                        priority = 7;
                    }
                    else if ((f != null) && (f is IncompleteFixture || f.HasComponent<Fixture>() || f is StructuralFeature))
                    {
                        priority = Math.Max(priority, 2);
                    }
                    else if (f != null && f.HasComponent<Harvestable>() && !(f is StructuralFeature)) // that last part is redundant but that's okay
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
            co.InfoMiddle = new List<ColoredText>() { txt };
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
                Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
                Feature? f = Features.GetWithBoundsChecked(x, y, z);
                Task? ta = Tasks.GetWithBoundsChecked(x, y, z);
                if (Explored.Contains(square) || HecatombOptions.Explored)
                {
                    // right now, incomplete dig tasks are the absolute top priority.  is that really what we want? or should they count the same as the terrain they're on?
                    if (f is IncompleteFixture && ta is null && (f as IncompleteFixture)!.Makes == typeof(Excavation))
                    {
                        priority = 7;
                    }
                    if (f != null && f.HasComponent<Harvestable>() && !(f is StructuralFeature))
                    {
                        priority = 6;
                    }
                    else if (!(f is null) && (f is IncompleteFixture || f.HasComponent<Fixture>() || f is StructuralFeature))
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
                // I accidentally double check them all
                if (Tasks.GetWithBoundsChecked(square.X, square.Y, square.Z) != null)
                {
                    continue;
                }
                int x = square.X;
                int y = square.Y;
                int z = square.Z;
                Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
                Feature? f = Features.GetWithBoundsChecked(x, y, z);
                Task? ta = Tasks.GetWithBoundsChecked(x, y, z);
                if (Explored.Contains(square) || HecatombOptions.Explored)
                {
                    // complete existing dig task
                    if (priority == 7 && f is IncompleteFixture && (f as IncompleteFixture)!.Makes == typeof(Excavation))
                    {
                        if (ta is null)
                        {
                            Entity.Spawn<DigTask>().PlaceInValidEmptyTile(x, y, z);
                        }
                    }
                    else if (priority == 6 && !(f is null) && f.HasComponent<Harvestable>() && !(f is StructuralFeature))
                    {
                        if (ta is null)
                        {
                            Publish(new TutorialEvent() { Action = "DesignateHarvestTask" });
                            Entity.Spawn<HarvestTask>().PlaceInValidEmptyTile(x, y, z);
                        }
                    }
                    else if (priority == 2 && ((f is IncompleteFixture && (f as IncompleteFixture)!.Makes != typeof(Excavation)) || (f != null && f.HasComponent<Fixture>()) || f is StructuralFeature))
                    {
                        // this arguably needs to be a differently named task, although I think this currently does the job
                        if (ta is null)
                        {
                            Entity.Spawn<HarvestTask>().PlaceInValidEmptyTile(x, y, z);
                        }
                    }
                    else if ((priority == 5 && (t == Terrain.WallTile || t == Terrain.UpSlopeTile))
                         || (priority == 4 && t == Terrain.FloorTile)
                         || (priority == 3 && t == Terrain.DownSlopeTile))
                    {
                        if (f == null)
                        {
                            // should I cancel existing tasks?
                            if (ta is null)
                            {
                                Publish(new TutorialEvent() { Action = "DesignateDigTask" });
                            }
                            Entity.Spawn<DigTask>().PlaceInValidEmptyTile(x, y, z);
                        }
                    }
                }
                else if (priority == 1 || priority == 5)
                {
                    // should I cancel existing tasks?
                    if (ta is null)
                    {
                        Publish(new TutorialEvent() { Action = "DesignateDigTask" });
                        Entity.Spawn<DigTask>().PlaceInValidEmptyTile(x, y, z);
                    }
                }
            }
        }

        public override bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            // what about non-harvestable, i.e. owned features?
            // maybe make those the very lowest priority?
            // in order to avoid giving away unexplored terrain, always allow designation
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return true;
            }
            Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
            Terrain tb = Terrains.GetWithBoundsChecked(x, y, z - 1);
            // can't dig an empty tile no matter what
            if (t == Terrain.EmptyTile)
            {
                return false;
            }
            // I think this is the correct priority for harvesting
            // oh wait, I made this a separate task...urgh...
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f != null)
            {
                if (f.HasComponent<Harvestable>())
                {
                    return true;
                }
                else if (f is IncompleteFixture)
                {
                    return true;
                }
                else if (f.HasComponent<Fixture>())
                {
                    return true;
                }
                else if (f is StructuralFeature)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // can't dig through the bottom of the level
            if (t == Terrain.FloorTile && tb == Terrain.VoidTile)
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
            Terrain tn = Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z);
            int hardness;
            if (tn == Terrain.WallTile || tn == Terrain.UpSlopeTile)
            {
                hardness = Covers.GetWithBoundsChecked(c.X, c.Y, c.Z).Hardness;
            }
            else
            {
                hardness = Covers.GetWithBoundsChecked(c.X, c.Y, c.Z - 1).Hardness;
            }
            if (HecatombOptions.IgnoreHardness || GetState<ResearchHandler>().GetToolHardness() >= hardness)
            {
                return false;
            }
            return true;
        }

        public override bool CanAssign(Creature c)
        {
            if (!Spawned || !Placed || !c.Spawned || !c.Placed)
            {
                return false;
            }
            var (x, y, z) = GetPlacedCoordinate();
            Coord crd = new Coord(x, y, z);
            if (!Explored.Contains(crd) && !HecatombOptions.Explored)
            {
                return false;
            }
            if (!ValidTile(crd))
            {
                if (TooHard(crd))
                {
                    PushMessage("Canceling dig task; material too hard.");
                }
                else
                {
                    PushMessage("Canceling invalid dig task.");
                }
                Cancel();
                
                return false;
            }
            // ValidTile produces misleading results if a harvest or deconstruct task could have been placed
            else if (Features.GetWithBoundsChecked(x, y, z) != null)
            {
                Feature f = Features.GetWithBoundsChecked(x, y, z)!;
                if (!(f is IncompleteFixture) || (f as IncompleteFixture)!.Makes != typeof(Excavation))
                {
                    PushMessage("Canceling invalid dig task.");
                    Cancel();
                    return false;
                }
            }
            Movement m = c.GetComponent<Movement>();
            return m.CanReachBounded(this, useLast: (WorkSameTile)) && m.CanReachResources(Ingredients);
        }
    }
}
