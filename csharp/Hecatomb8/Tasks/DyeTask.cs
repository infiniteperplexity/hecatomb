using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class DyeTask : Task, IDisplayInfo, IMenuListable
    {
        public bool? Background;
        public Resource? Dye;
        protected override string getName()
        {
            var handler = GetState<PaletteHandler>();
            if (Dye is null)
            {
                return "undye";
            }
            else if (Background is null)
            {
                return "dye with " + Dye.Name;
            }
            else if (Background == true)
            {
                return "dye background with " + Dye.Name;
            }
            else
            {
                return "dye foreground with " + Dye.Name;
            }
        }

        public override ColoredText ListOnMenu()
        {
            if (Dye is null)
            {
                return base.ListOnMenu();
            }
            if (Ingredients.Count != 0 && CanFindResources(Ingredients))
            {
                if (HecatombOptions.NoIngredients)
                {
                    return ("{" + Dye!.Name + "}" + _name);
                }
                else
                {
                    return ("{" + Dye!.Name + "}" + _name + " ($: " + Resource.Format(Ingredients) + ")");
                }
            }
            else
            {
                return base.ListOnMenu();
            }
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            menu.Header = "Dye a tile or feature:";
            var handler = GetState<PaletteHandler>();
            var list = new List<IMenuListable>();
            if (Dye is null)
            {
                foreach (var flower in Resource.Flowers)
                {
                    var task = Entity.Mock<DyeTask>();
                    task.Ingredients = new JsonArrayDictionary<Resource, int>() { [flower] = 1 };
                    task.Dye = flower;
                    list.Add(task);
                }
                var undye = Entity.Mock<DyeTask>();
                list.Add(undye);
            }
            else
            {
                var task = Entity.Mock<DyeTask>();
                task.Ingredients = new JsonArrayDictionary<Resource, int>() { [Dye] = 1 };
                task.Makes = Makes;
                task.Dye = Dye;
                list.Add(task);
                task.Background = false;
                list.Add(task);
                task = Entity.Mock<DyeTask>();
                task.Ingredients = new JsonArrayDictionary<Resource, int>() { [Dye] = 1 };
                task.Makes = Makes;
                task.Dye = Dye;
                list.Add(task);
                task.Background = true;
                list.Add(task);
            }
            menu.Choices = list;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {

        }


        public DyeTask() : base()
        {
            // I'd love to be able to dye tiles but currently that's hard
            _name = "dye a tile or feature";
            Priority = 4;
            // just keep this on Black Market for now
            RequiresStructures = new List<Type> { typeof(Apothecary) };
            _bg = "#0088BB";
        }

        public override void Start()
        {
            // don't place a new feature
        }
        public override void Finish()
        {
            if (!Placed || !Spawned)
            {
                return;
            }
            var (x, y, z) = GetVerifiedCoord();
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            var handler = GetState<PaletteHandler>();
            if (Dye is null)
            {
                if (f != null && f.HasComponent<CosmeticComponent>())
                {
                    if (f is Masonry)
                    {
                        f.Despawn();
                    }
                    else
                    {
                        var c = f.GetComponent<CosmeticComponent>();
                        c.RemoveFromEntity();
                        c.Despawn();
                    }
                }
            }
            else
            {
                if (f == null)
                {
                    var masonry = Entity.Spawn<Masonry>();
                    masonry.PlaceInValidEmptyTile(x, y, z);
                    Cover.ClearGroundCover(x, y, z);
                    f = masonry;
                }
                CosmeticComponent dyed;
                if (!f.HasComponent<CosmeticComponent>())
                {
                    dyed = Entity.Spawn<CosmeticComponent>();
                    f.AddComponent(dyed);
                }
                else
                {
                    dyed = f.GetComponent<CosmeticComponent>();
                }
                if ((bool)Background!)
                {
                    dyed.BG = Dye.FG;
                }
                else
                {
                    dyed.FG = dyed.BG = Dye.FG;
                }
            }
            Publish(new AchievementEvent() { Action = "FinishDyeTask" });
            Complete();
        }

        public override void ChooseFromMenu()
        {
            Publish(new TutorialEvent() { Action = "ChooseAnotherTask" });
            if (Dye is null)
            {
                var c = new InfoDisplayControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(c);
            }
            else if (Background == false && Makes != null)
            {
                var c = new InfoDisplayControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(c);
            }
            else
            {
                var c = new SelectZoneControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Jobs";
                InterfaceState.SetControls(c);
            }
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            var handler = GetState<PaletteHandler>();
            string txt;
            if (!ValidTile(c))
            {
                txt = (Dye is null) ? "undye" : "dye";
                co.InfoMiddle = new List<ColoredText>() { "{orange}Can't " + txt + " here." };
            }
            else
            {
                txt = (Dye is null) ? "Undye" : ("{" + Dye.FG + "}" + "Dye");
                co.InfoMiddle = new List<ColoredText>() { txt + String.Format(" from {0} {1} {2}.", c.X, c.Y, c.Z) };
            }
        }

        public override void SelectZone(List<Coord> squares)
        {
            CommandLogger.LogCommand(command: "DyeTask", makes: (Dye is null) ? null : Dye.TypeName, squares: squares, n: (Background == true) ? 1 : 0);
            var handler = GetState<PaletteHandler>();
            foreach (Coord c in squares)
            {
                if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) == null && ValidTile(c))
                {
                    Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
                    var task = Entity.Spawn<DyeTask>();
                    if (Dye != null)
                    {
                        task.Ingredients = new JsonArrayDictionary<Resource, int>() { { Dye, 1 } };
                    }
                    task.Makes = Makes;
                    task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    task.Background = Background;
                }
            }
        }

        public override bool ValidTile(Coord c)
        {
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
            Terrain t = Terrains.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (Dye is null && (f is null || !f.HasComponent<CosmeticComponent>()))
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

