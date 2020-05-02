using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Creature : ComposedEntity, IDisplayInfo
    {
        Species Species;
        protected Creature()
        {
            Species = Species.NoSpecies;
            Components.Add(new Movement());
            Components.Add(new Actor());
            Components.Add(new Senses());
        }
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Creatures.GetWithBoundsChecked(x, y, z) != null)
            {
                throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Creatures.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
            }
            Remove();
            GameState.World!.Creatures.SetWithBoundsChecked(x, y, z, this);
            base.PlaceInValidEmptyTile(x, y, z);
        }

        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Creatures.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            //menu.Header = "Creature: " + Describe();
            menu.Choices = new List<IMenuListable>();
            menu.SelectedEntity = this;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.InfoTop.RemoveAt(2);
            menu.InfoTop.Add("Tab) View minions.");
            menu.InfoTop.Add(" ");
            menu.InfoTop.Add("{yellow}" + Describe(capitalized: true));
            //if (HasComponent<Minion>() != null)
            //{
            //    menu.InfoTop.Add(" ");
            //    Task t = GetComponent<Minion>().Task;
            //    if (t == null)
            //    {
            //        menu.MenuTop.Add("No task assigned.");
            //    }
            //    else
            //    {
            //        menu.MenuTop.Add($"Working on {t.Describe(article: false)} at {t.X} {t.Y} {t.Z}");
            //    }
            //}
            //if (TryComponent<Inventory>() != null)
            //{
            //    menu.MenuTop.Add(" ");
            //    Item item = GetComponent<Inventory>().Item;
            //    if (item == null)
            //    {
            //        menu.MenuTop.Add("Carrying nothing.");
            //    }
            //    else
            //    {
            //        menu.MenuTop.Add("Carrying " + item.Describe());
            //    }
            //}
            if (this == Player)
            {
                var p = Player;
                menu.InfoTop.Add(" ");
                menu.InfoTop.Add(p.GetComponent<SpellCaster>().GetSanityText());
                //if (Game.World.GetState<TaskHandler>().Minions.Count > 0)
                //{
                //    menu.MenuTop.Add(" ");
                //    menu.MenuTop.Add("Minions:");
                //    var types = new Dictionary<string, int>();
                //    foreach (var minion in Game.World.GetState<TaskHandler>().Minions)
                //    {
                //        Creature c = (Creature)minion;
                //        if (!types.ContainsKey(c.TypeName))
                //        {
                //            types[c.TypeName] = 1;
                //        }
                //        else
                //        {
                //            types[c.TypeName] += 1;
                //        }
                //    }
                //    foreach (var type in types.Keys)
                //    {
                //        var mock = Entity.Mock<Creature>(type);
                //        // might need better handling for when we have multiple zombie types that still share a TypeName?
                //        menu.MenuTop.Add("{" + mock.FG + "}" + type + ": " + types[type]);
                //    }
                //}
            }
            menu.KeyMap[Keys.Escape] =
                () =>
                {
                    InterfaceState.ResetControls();
                    InterfaceState.Cursor = null;
                };
            menu.KeyMap[Keys.Tab] = NextMinion;
            menu.KeyMap[Keys.U] = Commands.ShowStructures;
            menu.KeyMap[Keys.M] = Commands.ShowMinions;
        }

        public void NextMinion()
        {
            //ControlContext.Selection = null;
            //var minions = GetState<TaskHandler>().Minions;
            //if (this == Player)
            //{
            //    if (minions.Count > 0)
            //    {
            //        ControlContext.Set(new MenuCameraControls((Creature)minions[0]));
            //        Game.Camera.CenterOnSelection();
            //    }
            //    else
            //    {
            //        ControlContext.Set(new MenuCameraControls(Player));
            //        Game.Camera.CenterOnSelection();
            //    }
            //}
            //else if (TryComponent<Minion>() == null)
            //{
            //    ControlContext.Set(new MenuCameraControls(Player));
            //    Game.Camera.CenterOnSelection();
            //}
            //else
            //{
            //    int n = -1;

            //    for (int i = 0; i < minions.Count; i++)
            //    {
            //        if (minions[i] == this)
            //        {
            //            n = i;
            //        }
            //    }
            //    if (n == -1 || n == minions.Count - 1)
            //    {
            //        //ControlContext.Set(new MenuChoiceControls(Player));
            //        ControlContext.Set(new MenuCameraControls(Player));
            //        Game.Camera.CenterOnSelection();
            //    }
            //    else
            //    {
            //        //ControlContext.Set(new MenuChoiceControls((Creature)minions[n+1]));

            //        ControlContext.Set(new MenuCameraControls((Creature)minions[n + 1]));
            //        Game.Camera.CenterOnSelection();
            //    }
            //}
        }
    }
}
