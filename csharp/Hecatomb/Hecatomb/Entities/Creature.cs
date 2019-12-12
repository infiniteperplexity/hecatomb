using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    using static HecatombAliases;

    public partial class Creature : TypedEntity, IChoiceMenu
    {
        private Actor cachedActor;

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            if (x1 == -1)
            {
                Debug.WriteLine("What on earth just happened?");
                Debug.WriteLine(this.Describe());
            }
            Creature e = Game.World.Creatures[x1, y1, z1];

            if (e == null)
            {
                //if (!fireEvent)
                //{
                //    Debug.Print("about to try placing {0} at {1} {2} {3} without firing an event", this, x1, y1, z1);
                //}
                Game.World.Creatures[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);

            }
            else
            {
                if (e == this)
                {
                    throw new InvalidOperationException(String.Format(
                        "Cannot place {0} at {1} {2} {3} because it has already been placed there.", TypeName, x1, y1, z1));
                }
                 throw new InvalidOperationException(String.Format(
                    "Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
                ));
            }
        }

        public override void Fall()
        {
            Movement m = TryComponent<Movement>();
            if (m == null || !m.Flies)
            {
                if (m != null && Covers[X, Y, Z].Liquid && m.Swims)
                {
                    return;
                }
                Place(X, Y, Z - 1);
                base.Fall();
            }
        }
        public override void Remove()
        {
            int x0 = X;
            int y0 = Y;
            int z0 = Z;
            base.Remove();
            Game.World.Creatures[x0, y0, z0] = null;
        }

        public override void Destroy(string cause = null)
        {
            int x = X;
            int y = Y;
            int z = Z;
            if (this == Game.World.Player)
            {
                Game.Commands.PlayerDies();
                return;
            }
            bool decaying = (TryComponent<Decaying>() != null);
            base.Destroy(cause: cause);
            if (!decaying)
            {
                Item.SpawnCorpse().Place(x, y, z);
            }
            else
            {
                Item.PlaceNewResource("Bone", 1, x, y, z);
            }
        }

        public void BuildMenu(MenuChoiceControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            //menu.Header = "Creature: " + Describe();
            menu.Choices = new List<IMenuListable>();
            ControlContext.Selection = this;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.RemoveAt(2);
            menu.MenuTop.Add("Tab) View minions.");
            menu.MenuTop.Add(" ");
            menu.MenuTop.Add("{yellow}" + Describe(capitalized: true));
            if (TryComponent<Minion>() != null)
            {
                menu.MenuTop.Add(" ");
                Task t = GetComponent<Minion>().Task;
                if (t == null)
                {
                    menu.MenuTop.Add("No task assigned.");
                }
                else
                {
                    menu.MenuTop.Add($"Working on {t.Describe()} at {t.X} {t.Y} {t.Z}");
                }
            }
            if (TryComponent<Inventory>() != null)
            {
                menu.MenuTop.Add(" ");
                Item item = GetComponent<Inventory>().Item;
                if (item == null)
                {
                    menu.MenuTop.Add("Carrying nothing.");
                }
                else
                {
                    menu.MenuTop.Add("Carrying " + item.Describe());
                }
            }
            if (this == Player)
            {
                var p = Game.World.Player;
                menu.MenuTop.Add(" ");
                menu.MenuTop.Add(p.GetComponent<SpellCaster>().GetSanityText());
                if (Game.World.GetState<TaskHandler>().Minions.Count > 0)
                {
                    menu.MenuTop.Add(" ");
                    menu.MenuTop.Add("Minions:");
                    var types = new Dictionary<string, int>();
                    foreach (var minion in Game.World.GetState<TaskHandler>().Minions)
                    {
                        Creature c = (Creature)minion;
                        if (!types.ContainsKey(c.TypeName))
                        {
                            types[c.TypeName] = 1;
                        }
                        else
                        {
                            types[c.TypeName] += 1;
                        }
                    }
                    foreach (var type in types.Keys)
                    {
                        var mock = Entity.Mock<Creature>(type);
                        // might need better handling for when we have multiple zombie types that still share a TypeName?
                        menu.MenuTop.Add("{" + mock.FG + "}" + type + ": " + types[type]);
                    }
                }
            }
            menu.KeyMap[Keys.Escape] =
                () =>
                {
                    ControlContext.Selection = null;
                    ControlContext.Reset();
                };
            menu.KeyMap[Keys.Tab] = NextMinion;
            Game.Camera.Center(X, Y, Z);
        }

        public Actor GetCachedActor()
        {
            if (cachedActor == null)
            {
                cachedActor = GetComponent<Actor>();
            }
            return cachedActor;
        }

        public void NextMinion()
        {
            ControlContext.Selection = null;
            var minions = GetState<TaskHandler>().Minions;
            if (this==Player)
            {
                if (minions.Count>0)
                {
                    ControlContext.Set(new MenuChoiceControls((Creature) minions[0]));
                }
            }
            else if (TryComponent<Minion>()==null)
            {
                ControlContext.Set(new MenuChoiceControls(Player));
            }
            else
            {
                int n = -1;
                
                for (int i=0; i<minions.Count; i++)
                {
                    if (minions[i]==this)
                    {
                        n = i;
                    }
                }
                if (n==-1 || n==minions.Count-1)
                {
                    ControlContext.Set(new MenuChoiceControls(Player));
                }
                else
                {
                    ControlContext.Set(new MenuChoiceControls((Creature)minions[n+1]));
                }
            }
        }

        public override string GetDisplayName()
        {
            var defend = TryComponent<Defender>();
            int wounds = (defend == null) ? 0 : defend.Wounds;
            var decay = TryComponent<Decaying>();
            double rotten = (decay == null) ? 1.0 : decay.GetFraction();
            if (wounds >= 6)
            {
                return ("severely wounded " + base.GetDisplayName());
            }
            else if (rotten < 0.25)
            {
                return ("severely rotted " + base.GetDisplayName());
            }
            else if (wounds >= 4)
            {
                return ("wounded " + base.GetDisplayName());
            }
            else if (rotten < 0.5)
            {
                return ("rotted " + base.GetDisplayName());
            }
            else if (wounds >= 2)
            {
                return ("slightly wounded " + base.GetDisplayName());
            }
            else if (rotten < 0.75)
            {
                return ("slightly rotted " + base.GetDisplayName());
            }
            return base.GetDisplayName();
        }

    }
}
