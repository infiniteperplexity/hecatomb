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
            Components.Add(new Actor() { Activities = new List<Activity>() { Activity.Default} });
            Components.Add(new Senses());
        }
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Creatures.GetWithBoundsChecked(x, y, z) != null)
            {
                if (HecatombOptions.NoisyErrors)
                {
                    throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Creatures.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
                }
                else
                {
                    var cr = GameState.World!.Creatures.GetWithBoundsChecked(x, y, z)!;
                    if (cr == Player)
                    {
                        Despawn();
                        return;
                    }
                    else
                    {
                        cr.Despawn();
                    }
                }
            }
            base.PlaceInValidEmptyTile(x, y, z);
            if (Spawned)
            {
                GameState.World!.Creatures.SetWithBoundsChecked(x, y, z, this);
                Publish(new AfterPlaceEvent() { Entity = this, X = x, Y = y, Z = z });
            }
        }

        public virtual void Act()
        {

        }

        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Creatures.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
            base.Remove();
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            //menu.Header = "Creature: " + Describe();
            menu.Choices = new List<IMenuListable>();
            menu.SelectedEntity = this;

            // we keep this next bit unless we add menu choices
            var Commands = InterfaceState.Commands!;
            menu.MenuCommandsSelectable = true;
            menu.KeyMap[Keys.Z] = Commands.ChooseSpell;
            menu.KeyMap[Keys.J] = Commands.ChooseTask;
            menu.KeyMap[Keys.R] = Commands.ShowResearch;
            menu.KeyMap[Keys.L] = Commands.ShowLog;
            menu.KeyMap[Keys.V] = Commands.ShowAchievements;
        }
        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.InfoTop.RemoveAt(2);
            menu.InfoTop.Add("Tab) View minions.");
            menu.InfoTop.Add(" ");
            menu.InfoTop.Add("{yellow}" + Describe(capitalized: true));
            if (HasComponent<Minion>())
            {
                menu.InfoTop.Add(" ");
                Task? t = GetComponent<Minion>().Task?.UnboxBriefly();
                if (t == null)
                {
                    menu.InfoTop.Add("No task assigned.");
                }
                else
                {
                    menu.InfoTop.Add($"Working on {t.Describe(article: false)} at {t.X} {t.Y} {t.Z}");
                }
            }
            if (HasComponent<Inventory>())
            {
                menu.InfoTop.Add(" ");
                Item? item = GetComponent<Inventory>().Item?.UnboxBriefly();
                if (item is null)
                {
                    menu.InfoTop.Add("Carrying nothing.");
                }
                else
                {
                    menu.InfoTop.Add("Carrying " + item.Describe());
                }
            }
            if (this == Player)
            {
                // originally I was duplicating part of the default display but that's no longer necessary
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
