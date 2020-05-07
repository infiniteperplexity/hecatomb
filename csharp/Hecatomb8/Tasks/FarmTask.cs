using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb8
{
    using static HecatombAliases;
    public class FarmTask : Task, IDisplayInfo, IMenuListable
    {
        Resource? Flower;
        protected override string getName()
        {
            Feature f;
            if (Flower != null)
            {
                f = Hecatomb8.Flower.Mock(Flower);
            }
            else if (Makes != null)
            {
                f = (Feature)Entity.Mock(Makes);
            }
            else
            {
                return "farm task";
            }
            return $"plant {f.Describe()}";
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            menu.Header = "Plant a seedling:";
            var list = new List<IMenuListable>();
            var structures = Structure.ListStructureTypes();
            var researched = GetState<ResearchHandler>().Researched;
            var task = Entity.Mock<FarmTask>();
            task.Makes = typeof(Sapling);
            list.Add(task);
            foreach (var flower in Resource.Flowers)
            {
                task = Entity.Mock<FarmTask>();
                task.Flower = flower;
                task.Makes = typeof(Hecatomb8.Flower);
                task.Ingredients = new JsonArrayDictionary<Resource, int>() { { flower, 1 } };
                list.Add(task);
            }
            menu.Choices = list;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {

        }


        public FarmTask() : base()
        {
            _name = "plant a seedling";
            Priority = 4;
            RequiresStructures = new List<Type> { typeof(Apothecary) };
            _bg = "dark green";
        }

        public override void Start()
        {
            // no incomplete feature
        }

        public override void Finish()
        {
            if (!Placed || !Spawned)
            {
                return;
            }
            var (x, y, z) = GetPlacedCoordinate();
            var f = Features.GetWithBoundsChecked(x, y, z);
            if (f != null)
            {
                return;
            }
            if (Flower != null)
            {
                f = Hecatomb8.Flower.Spawn(Flower);
            }
            else if (Makes != null)
            {
                f = (Feature)Entity.Spawn(Makes);
            }
            f?.PlaceInValidEmptyTile(x, y, z);
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
                c.SelectedMenuCommand = "Jobs";
                c.InfoMiddle = new List<ColoredText>() { "{green}Plant " + ((Flower is null) ? "a sapling" : Flower.Name) + "." };
                InterfaceState.SetControls(c);
            }
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            co.InfoMiddle = new List<ColoredText>() { "{green}Plant " + ((Flower is null) ? "a sapling" : Flower.Name) + String.Format(" at {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "FarmingTask", makes: Makes?.Name, x: c.X, y: c.Y, z: c.Z);
            if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null && ValidTile(c))
            {
                FarmTask task = Entity.Spawn<FarmTask>();
                task.Ingredients = new JsonArrayDictionary<Resource, int>();
                // eventually we might have things that have specified ingredients
                if (Flower != null)
                {
                    task.Flower = Flower;
                    task.Ingredients[Flower] = 1;
                }
                task.Makes = Makes;
                task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
            }
        }
    }
}