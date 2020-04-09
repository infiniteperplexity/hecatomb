
/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    public class DyeTask : Task, IChoiceMenu, IMenuListable
    {
        public bool? Background;
        // so...not being able to dye walls and floors is actually an extreme limitation
        // what do we need in order to make it okay?  
        // basically we need a lot of changes to the dig task, and perhaps to a few related methods (e.g. mine, tile validation.)
        // I briefly thought of doing it with Cover, which lines up well with how ores work...but ores have fixed backgrounds (and symbols, for that matter)
        public override string GetDisplayName()
        {
            var handler = Game.World.GetState<RandomPaletteHandler>();
            if (Makes == "Undye")
            {
                return "undye";
            }
            return "dye with " + handler.GetFlowerName(Makes);
        }

        public override string GetHoverName()
        {
            var handler = Game.World.GetState<RandomPaletteHandler>();
            if (Makes == "Undye")
            {
                return "undye";
            }
            return "dye with " + handler.GetFlowerName(Makes);
        }

        public override ColoredText ListOnMenu()
        {
            if (cachedMenuListing == null && Ingredients.Count != 0 && Game.World.Player.GetComponent<Movement>().CanFindResources(Ingredients, useCache: false))
            {
                var handler = Game.World.GetState<RandomPaletteHandler>();
                if (Game.Options.NoIngredients)
                {
                    cachedMenuListing = ("{" + handler.GetFlowerColor(Makes) + "}" + MenuName);
                }
                else
                {
                    cachedMenuListing = ("{" + handler.GetFlowerColor(Makes) + "}" + MenuName + " ($: " + Resource.Format(Ingredients) + ")");
                }
                return cachedMenuListing;
            }
            else
            {
                return base.ListOnMenu();
            }
        }

        protected List<IMenuListable> cachedChoices;
        public void BuildMenu(MenuChoiceControls menu)
        {
            if (cachedChoices != null)
            {
                menu.Choices = cachedChoices;
                return;
            }
            menu.Header = "Dye a tile or feature:";
            var handler = Game.World.GetState<RandomPaletteHandler>();
            var list = new List<IMenuListable>();
            if (Makes == null)
            {
                foreach (var tuple in RandomPaletteHandler.FlowerNames)
                {
                    var task = Hecatomb.Entity.Mock<DyeTask>();
                    task.Ingredients = new Dictionary<string, int>() { };
                    task.Makes = tuple.Item1;
                    task.MenuName = handler.GetFlowerName(task.Makes);
                    task.Ingredients[task.Makes] = 1;
                    list.Add(task);
                }
                var undye = Hecatomb.Entity.Mock<DyeTask>();
                undye.Makes = "Undye";
                undye.MenuName = "remove dye";
                list.Add(undye);
            }
            else
            {
                var task = Hecatomb.Entity.Mock<DyeTask>();
                task.Ingredients = new Dictionary<string, int>() { };
                task.Makes = Makes;
                task.MenuName = "foreground with " + handler.GetFlowerName(task.Makes);
                task.Ingredients[task.Makes] = 1;
                task.Background = false;
                list.Add(task);
                task = Hecatomb.Entity.Mock<DyeTask>();
                task.Ingredients = new Dictionary<string, int>() { };
                task.Makes = Makes;
                task.MenuName = "background with " + handler.GetFlowerName(task.Makes);
                task.Ingredients[task.Makes] = 1;
                task.Background = true;
                list.Add(task);
            }
            
            cachedChoices = list;
            menu.Choices = list;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }


        public DyeTask() : base()
        {
            // I'd love to be able to dye tiles but currently that's hard
            MenuName = "dye a tile or feature";
            Priority = 4;
            // just keep this on Black Market for now
            PrereqStructures = new List<string> { "Apothecary" };
            BG = "#0088BB";
        }

        public override void Start()
        {
            // don't place a new feature
        }
        public override void Finish()
        {
            Feature f = Game.World.Features[X, Y, Z];
            var handler = Game.World.GetState<RandomPaletteHandler>();
            if (Makes == "Undye")
            {
                if (f!=null && f.TryComponent<DyedComponent>()!=null)
                {
                    if (f.TypeName == "Masonry")
                    {
                        f.Despawn();
                    }
                    else
                    {
                        var c = f.GetComponent<DyedComponent>();
                        c.RemoveFromEntity();
                        c.Despawn();
                    }
                }
            }
            else
            {
                if (f == null)
                {
                    var masonry = Entity.Spawn<Feature>("Masonry");
                    masonry.Place(X, Y, Z);
                    Cover.ClearCover(X, Y, Z);
                    f = masonry;
                }
                var dyed = f.TryComponent<DyedComponent>();
                if (dyed==null)
                {
                    dyed = Entity.Spawn<DyedComponent>();
                    f.AddComponent(dyed);
                }
                if ((bool) Background)
                {
                    dyed.BG = handler.FlowerColors[Makes];
                }
                else
                {
                    dyed.FG = handler.FlowerColors[Makes];
                }
            }
            Game.World.Events.Publish(new AchievementEvent() { Action = "FinishDyeTask" });
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
            else if (Background == null && Makes != "Undye")
            {
                var c = new MenuChoiceControls(this);
                c.MenuSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                ControlContext.Set(c);
            }
            else
            {
                var c = new SelectZoneControls(this);
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
            string txt;
            if (!ValidTile(c))
            {
                txt = (Makes == "Undye") ? "undye" : "dye";
                co.MenuMiddle = new List<ColoredText>() { "{orange}Can't " + txt + " here." };
            }
            else
            {
                txt = (Makes == "Undye") ? "Undye" : ("{" + handler.FlowerColors[Makes] + "}" + "Dye");
                co.MenuMiddle = new List<ColoredText>() {txt +  String.Format(" from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
        }

        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "DyeTask", makes: Makes, squares: squares, n: (Background == true) ? 1 : 0);
            var handler = Game.World.GetState<RandomPaletteHandler>();
            foreach (Coord c in squares)
            {
                if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
                {
                    Feature f = Game.World.Features[c];
                    var task = Hecatomb.Entity.Spawn<DyeTask>();
                    task.Ingredients = new Dictionary<string, int>() { { Makes, 1 } };
                    task.Makes = Makes;
                    task.Place(c.X, c.Y, c.Z);
                    task.Background = Background;
                }
            }
        }

        public override bool ValidTile(Coord c)
        {
            if (!Game.World.Explored.Contains(c) && !Game.Options.Explored)
            {
                return false;
            }
            Feature f = Game.World.Features[c];
            Terrain t = Game.World.Terrains[c.X, c.Y, c.Z];
            if (Makes == "Undye" && (f == null || f.TryComponent<DyedComponent>()==null))
            {
                return false;
            }
            else if (f != null || t == Terrain.FloorTile || t == Terrain.WallTile || t == Terrain.UpSlopeTile)
            {
                return true;
            }
            return false;
        }
    }

}

