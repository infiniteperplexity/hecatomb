using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class FurnishTask : Task, IDisplayInfo, IMenuListable
    {
        Type[] Fixtures;

        protected override string getName()
        {
            if (Makes is null)
            {
                return _name!;
            }
            Feature f = (Feature)Entity.Mock(Makes);
            return $"furnish {f.Name}";
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            menu.Header = "Furnish a fixture:";
            var list = new List<IMenuListable>();
            var structures = Structure.ListStructureTypes();
            var researched = GetState<ResearchHandler>().Researched;
            // only if we have the prerequisite structures / technologies...
            foreach (Type t in Fixtures)
            {
                Feature f = (Feature)Entity.Mock(t);
                Fixture fix = f.GetPrespawnComponent<Fixture>();
                bool valid = true;

                foreach (Research s in fix.RequiresResearch)
                {
                    if (!researched.Contains(s))
                    {
                        valid = false;
                    }
                }
                foreach (Type s in fix.RequiresStructures)
                {
                    if (!structures.Contains(s))
                    {
                        valid = false;
                    }
                }
                if (valid || HecatombOptions.NoIngredients)
                {
                    var task = Entity.Mock<FurnishTask>();
                    task.Ingredients = new JsonArrayDictionary<Resource, int>(fix.Ingredients);
                    task.Makes = t;
                    list.Add(task);
                }
            }
            var repair = Entity.Mock<RepairTask>();
            repair.Makes = typeof(Feature);
            list.Add(repair);
            menu.Choices = list;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {

        }


        public FurnishTask() : base()
        {
            _name = "build or repair a fixture";
            Priority = 4;
            Fixtures = new Type[] {
                typeof(Door),
                typeof(Masonry),
                typeof(Ramp),
                typeof(SpearTrap)
            };
            RequiresStructures = new List<Type> { typeof(Workshop) };
            _bg = "yellow";
        }

        public override void Finish()
        {
            if (!Spawned || !Placed)
            {
                return;
            }
            var (x, y, z) = GetValidCoordinate();
            Publish(new TutorialEvent() { Action = "AnyBuildComplete" });
            Feature? incomplete = Features.GetWithBoundsChecked(x, y, z);
            incomplete?.Despawn();
            // maybe some features should allow grass to stay?
            Cover.ClearGroundCover(x, y, z);
            Feature finished = Entity.Spawn<Feature>(Makes!);
            finished.PlaceInValidEmptyTile(x, y, z);
            base.Finish();
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            if (Makes == null)
            {
                var c = new InfoDisplayControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(c);

            }
            else
            {
                var c = new SelectTileControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                Feature f = (Feature)Entity.Mock(Makes);
                c.InfoMiddle = new List<ColoredText>() {"{green}Furnish " + f.Name};
                InterfaceState.SetControls(c);
            }
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Furnish {3} at {0} {1} {2}", c.X, c.Y, c.Z, ((Feature)Entity.Mock(Makes!)).Name) };
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "FurnishTask", makes: Makes!.Name, x: c.X, y: c.Y, z: c.Z);
            if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null && ValidTile(c))
            {
                Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
                Defender? d = null;
                if (f != null && f.HasComponent<Defender>())
                {
                    d = f.GetComponent<Defender>();
                }
                if (f != null && f.HasComponent<Fixture>() && d != null && f.GetType() == Makes && d.Wounds > 0)
                {
                    Task task = Entity.Spawn<RepairTask>();
                    task.Makes = typeof(Feature);
                    task.Ingredients = new JsonArrayDictionary<Resource, int>(f.GetComponent<Fixture>().Ingredients);
                    task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                }
                else
                {
                    Task task = Entity.Spawn<FurnishTask>();
                    Feature feat = (Feature)Entity.Mock(Makes);
                    Fixture fix = feat.GetPrespawnComponent<Fixture>();
                    task.Ingredients = new JsonArrayDictionary<Resource, int>(fix.Ingredients);
                    task.LaborCost = fix.Labor;
                    task.Labor = fix.Labor;
                    task.Makes = Makes;
                    task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                }
            }
        }

        public override bool ValidTile(Coord c)
        {
            Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
            Defender? d = f?.TryComponent<Defender>();
            if (f != null && f.HasComponent<Fixture>() && d != null && f.GetType() == Makes && d.Wounds > 0)
            {
                return true;
            }
            return base.ValidTile(c);
        }
    }

}

