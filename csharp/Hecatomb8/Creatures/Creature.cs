using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Creature : ComposedEntity, IDisplayInfo
    {
        [JsonIgnore] public Species Species;
        //[JsonIgnore] public bool LeavesCorpse;
        protected Creature()
        {
            Species = Species.NoSpecies;
            //LeavesCorpse = true;
            Components.Add(new Movement());
            Components.Add(new Actor() { Activities = new List<Activity>() { Activity.Default} });
            Components.Add(new Senses());
            Components.Add(new Attacker());
            Components.Add(new Defender());
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
            var minions = GetState<TaskHandler>().GetMinions();
            if (this == Player)
            {
                if (minions.Count > 0)
                {
                    InterfaceState.SetControls(new InfoDisplayControls((Creature)minions[0]));
                    InterfaceState.Camera!.CenterOnSelection();
                }
                else
                {
                    InterfaceState.SetControls(new InfoDisplayControls(Player!));
                    InterfaceState.Camera!.CenterOnSelection();
                }
            }
            else if (TryComponent<Minion>() == null)
            {
                InterfaceState.SetControls(new InfoDisplayControls(Player!));
                InterfaceState.Camera!.CenterOnSelection();
            }
            else
            {
                int n = -1;

                for (int i = 0; i < minions.Count; i++)
                {
                    if (minions[i] == this)
                    {
                        n = i;
                    }
                }
                if (n == -1 || n == minions.Count - 1)
                {
                    InterfaceState.SetControls(new InfoDisplayControls(Player!));
                    InterfaceState.Camera!.CenterOnSelection();
                }
                else
                {
                    InterfaceState.SetControls(new InfoDisplayControls((Creature)minions[n + 1]));
                    InterfaceState.Camera!.CenterOnSelection();
                }
            }
        }

        public override void Destroy(string? cause = null)
        {
            if (Spawned && Placed)
            {
                Species.Remains(this, cause);
            }
            base.Destroy(cause);
        }

        public static Coord? FindPlace(int x, int y, int z, int max = 5, int min = 0, bool groundLevel = true)
        {
            return Tiles.NearbyTile(x, y, z, max: max, min: min, valid: (fx, fy, fz) => { return (Creatures.GetWithBoundsChecked(fx, fy, fz) is null); });
        }

        protected override string? getName()
        {
            if (HasComponent<Defender>())
            {
                int Wounds = GetComponent<Defender>().Wounds;
                if (Wounds >= 6)
                {
                    return "severely wounded " + base.getName();
                }
                else if (Wounds >= 4)
                {
                    return "wounded " + base.getName();
                }
                else if (Wounds >= 2)
                {
                    return "slightly wounded " + base.getName();
                }
            }
            return base.getName();
        }
    }
}
