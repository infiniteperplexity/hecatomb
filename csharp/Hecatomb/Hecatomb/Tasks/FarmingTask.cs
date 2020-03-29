using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    public class FarmingTask : Task, IChoiceMenu, IMenuListable
    {
        public override string GetDisplayName()
        {
            Feature f;
            if (RandomPaletteHandler.FlowerDictionary.ContainsKey(Makes))
            {
                f = RandomPaletteHandler.MockFlower(Makes);
            }
            else
            {
                f = Entity.Mock<Feature>(Makes);
            }
            return $"plant {f.Describe()}";
        }

        protected List<IMenuListable> cachedChoices;
        public void BuildMenu(MenuChoiceControls menu)
        {
            if (cachedChoices != null)
            {
                menu.Choices = cachedChoices;
                return;
            }
            menu.Header = "Plant a seedling:";
            var list = new List<IMenuListable>();
            var structures = Structure.ListAsStrings();
            var researched = Game.World.GetState<ResearchHandler>().Researched;
            // only if we have the prerequisite structures / technologies...
            var task = Hecatomb.Entity.Mock<FarmingTask>();
            task.Makes = "Sapling";
            task.MenuName = "tree";
            list.Add(task);
            foreach (var tuple in RandomPaletteHandler.FlowerNames)
            {
                task = Hecatomb.Entity.Mock<FarmingTask>();
                task.Makes = tuple.Item1;
                task.Ingredients = new Dictionary<string, int>() { { tuple.Item1, 1} };
                task.MenuName = tuple.Item2;
                list.Add(task);
            }
            //list.Add(Hecatomb.Entity.Mock<RepairTask>());
            cachedChoices = list;
            menu.Choices = list;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }


        public FarmingTask() : base()
        {
            MenuName = "plant a seedling";
            Priority = 4;
            PrereqStructures = new List<string> { "Apothecary" };
            BG = "dark green";
        }

        public override void Start()
        {
          // no incomplete feature
        }

        public override void Finish()
        {
            Feature f;
            var handler = Game.World.GetState<RandomPaletteHandler>();
            Debug.WriteLine(Makes);
            if (RandomPaletteHandler.FlowerDictionary.ContainsKey(Makes))
            {
                Debug.WriteLine("flag 0");
                f = RandomPaletteHandler.SpawnFlower(Makes);
            }
            else
            {
                Debug.WriteLine("flag 1");
                f = Entity.Spawn<Feature>(Makes);
            }
            f.Place(X, Y, Z);
            base.Finish();
        }

        public override void ChooseFromMenu()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            if (Makes == null)
            {
                var c = new MenuChoiceControls(this);
                c.MenuSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                ControlContext.Set(c);
            }
            else
            {
                var c = new SelectTileControls(this);
                c.MenuSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                ControlContext.Set(c);
            }
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Plant seedling at {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "FarmingTask", makes: Makes, x: c.X, y: c.Y, z: c.Z);
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
            {
                Task task = Entity.Spawn<FarmingTask>();
                task.Ingredients = new Dictionary<string, int>();
                // eventually we might have things that have specified ingredients
                if (RandomPaletteHandler.FlowerDictionary.ContainsKey(Makes))
                {
                    Ingredients[Makes] = 1;
                }
                task.Makes = Makes;
                task.Place(c.X, c.Y, c.Z);
            }
        }
    }
}