
/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    public class ColorizeTask : Task, IChoiceMenu, IMenuListable
    {
        // so...not being able to dye walls and floors is actually an extreme limitation
        // what do we need in order to make it okay?  
        // basically we need a lot of changes to the dig task, and perhaps to a few related methods (e.g. mine, tile validation.)
        // I briefly thought of doing it with Cover, which lines up well with how ores work...but ores have fixed backgrounds (and symbols, for that matter)
        public override string GetDisplayName()
        {
            var handler = Game.World.GetState<RandomPaletteHandler>();
            return "{" + handler.FlowerColors[Makes] + "}" + "dye with " + handler.GetFlowerColorName(Makes);
        }

        protected List<IMenuListable> cachedChoices;
        public void BuildMenu(MenuChoiceControls menu)
        {
            if (cachedChoices != null)
            {
                menu.Choices = cachedChoices;
                return;
            }
            menu.Header = "Dye a tile or feature with:";
            var handler = Game.World.GetState<RandomPaletteHandler>();
            var list = new List<IMenuListable>();
            foreach (var tuple in RandomPaletteHandler.FlowerNames)
            {
                var task = Hecatomb.Entity.Mock<ColorizeTask>();
                task.Ingredients = new Dictionary<string, int>() { };
                task.Makes = tuple.Item1;
                task.MenuName = "{" + handler.FlowerColors[Makes] + "}" + handler.GetFlowerColorName(Makes);
                list.Add(task);
            }
            cachedChoices = list;
            menu.Choices = list;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }


        public ColorizeTask() : base()
        {
            // I'd love to be able to dye tiles but currently that's hard
            MenuName = "dye a tile or feature";
            Priority = 4;
            // just keep this on Black Market for now
            PrereqStructures = new List<string> { /*"Black Market"*/ };
            BG = "magenta";
        }

        public override void Start()
        {
            // don't place a new feature
        }
        public override void Finish()
        {
            // here's the meat of it
            Feature f = Game.World.Features[X, Y, Z];
            var handler = Game.World.GetState<RandomPaletteHandler>();
            if (f != null)
            {
                var c = Entity.Spawn<CustomizedComponent>();
                c.FG = handler.FlowerColors[Makes];
                f.AddComponent(c);
            }
            
            Complete();
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
            var handler = Game.World.GetState<RandomPaletteHandler>();
            co.MenuMiddle = new List<ColoredText>() { "{"+ handler.FlowerColors[Makes] +"}" + String.Format("Dye at {0} {1} {2}", c.X, c.Y, c.Z) };
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "ColorizeTask", makes: Makes, x: c.X, y: c.Y, z: c.Z);
            var handler = Game.World.GetState<RandomPaletteHandler>();
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
            {
                Feature f = Game.World.Features[c];
                var task = Hecatomb.Entity.Mock<ColorizeTask>();
                task.Ingredients = new Dictionary<string, int>() { };
                task.Makes = Makes;
                task.MenuName = "{" + handler.FlowerColors[Makes] + "}" + "dye with " + handler.GetFlowerColorName(Makes);
                task.Place(c.X, c.Y, c.Z);
            }
        }

        public override bool ValidTile(Coord c)
        {
            Feature f = Game.World.Features[c];
            Defender d = f?.TryComponent<Defender>();
            if (f != null && d != null && f.TypeName == Makes && d.Wounds > 0)
            {
                return true;
            }
            return base.ValidTile(c);
        }
    }

}

