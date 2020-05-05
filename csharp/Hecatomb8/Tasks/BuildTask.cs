using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class Construction : Feature
    {
        public Construction()
        {
            _name = "construction";

        }
    }
    public class BuildTask : Task
    {
        public BuildTask() : base()
        {
            _name = "build floors or walls";
            Makes = typeof(Construction);
            Ingredients = new JsonArrayDictionary<Resource, int>
            {
                {Resource.Rock, 1}
            };
            _bg = "purple";
        }

        protected override string getName()
        {
            if (!Spawned || !Placed)
            {
                return _name!;
            }
            var (x, y, z) = GetValidCoordinate();
            Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
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
            if (!Placed || !Spawned)
            {
                return;
            }
            var (x, y, z) = GetValidCoordinate();
            base.Start();
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f is IncompleteFixture)
            {
                var ifx = (f as IncompleteFixture)!;
                ifx.Makes = typeof(Construction);
                ifx.IncompleteSymbol = '\u2692';
                ifx.IncompleteFG = "white";
            }
        }
        public override void Finish()
        {
            if (!Placed || !Spawned)
            {
                return;
            }
            var (x, y, z) = GetValidCoordinate();
            Publish(new TutorialEvent() { Action = "BuildTaskComplete" });
            Features.GetWithBoundsChecked(x, y, z)?.Despawn();
            Publish(new TutorialEvent() { Action = "AnyBuildComplete" });
            Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
            Terrain floor = Terrain.FloorTile;
            Terrain wall = Terrain.WallTile;
            Terrain up = Terrain.UpSlopeTile;
            Terrain down = Terrain.DownSlopeTile;
            Terrain empty = Terrain.EmptyTile;
            if (t == floor || t == up)
            {
                Terrains.SetWithBoundsChecked(x, y, z, wall);
                // this might change eventually
                Covers.SetWithBoundsChecked(x, y, z, Cover.Soil);
                Terrain ta = Terrains.GetWithBoundsChecked(x, y, z + 1);
                if (ta == empty || ta == down)
                {
                    Terrains.SetWithBoundsChecked(x, y, z + 1, floor);
                }
            }
            else if (t == empty || t == down)
            {
                Terrains.SetWithBoundsChecked(x, y, z, floor);
            }
            base.Finish();
            GameState.World!.ValidateOutdoors();
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (ValidTile(c))
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Build walls or floors from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Can't build here." };
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
                if (Explored.Contains(square) || HecatombOptions.Explored)
                {
                    if (f is IncompleteFixture && (f as IncompleteFixture)!.Makes == typeof(Excavation))
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
            co.InfoMiddle = new List<ColoredText>() { txt };
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
                Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
                Feature? f = Features.GetWithBoundsChecked(x, y, z);
                if (Explored.Contains(square) || HecatombOptions.Explored)
                {
                    if (f is IncompleteFixture && (f as IncompleteFixture)!.Makes == typeof(Excavation))
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
                if (Tasks.GetWithBoundsChecked(x, y, z) != null)
                {
                    continue;
                }
                Terrain t = Terrains.GetWithBoundsChecked(x, y, z);
                Feature? f = Features.GetWithBoundsChecked(x, y, z);
                if (f is IncompleteFixture && (f as IncompleteFixture)!.Makes == typeof(Construction))
                {
                    Entity.Spawn<BuildTask>().PlaceInValidEmptyTile(x, y, z);
                }
                // filling in a prior excavation
                else if (priority == 3 && f is IncompleteFixture && (f as IncompleteFixture)!.Makes == typeof(Excavation))
                {
                    HarvestTask ht = Entity.Spawn<HarvestTask>();
                    ht.PlaceInValidEmptyTile(x, y, z);
                    ht.Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Rock, 1 } };
                }
                else if ((priority == 2 && (t == Terrain.EmptyTile || t == Terrain.DownSlopeTile))
                      || (priority == 1 && t == Terrain.FloorTile || t == Terrain.UpSlopeTile))
                {
                    if (f is null)
                    {
                        Publish(new TutorialEvent() { Action = "DesignateBuildTask" });
                        Entity.Spawn<BuildTask>().PlaceInValidEmptyTile(x, y, z);
                    }
                }
            }
        }


        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseBuildTask" });
            var c = new SelectZoneControls(this);
            c.MenuCommandsSelectable = false;
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Build walls or floors." };
            InterfaceState.SetControls(c);
        }

        public override bool ValidTile(Coord c)
        {
            Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            // can't build on most features
            if (f != null)
            {
                if ((f is IncompleteFixture) && ((f as IncompleteFixture)!.Makes == typeof(Excavation) || (f as IncompleteFixture)!.Makes == typeof(Construction)))
                {
                    return true;
                }
                return false;
            }
            // can't build on a wall tile
            if (Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z) == Terrain.WallTile)
            {
                return false;
            }
            return true;
        }
    }
}
