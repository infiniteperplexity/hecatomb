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

        public string Species;

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
                Displace(x1, y1, z1);
            }
        }

        public void Displace(int x, int y, int z)
        {
            int MaxDistance = 2;
            List<int> order = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            order = order.OrderBy(s => Game.World.Random.Arbitrary(OwnSeed() + s)).ToList();
            //order = order.OrderBy(s => Game.World.Random.NextDouble()).ToList();
            Queue<Coord> queue = new Queue<Coord>();
            if (Terrains[x, y, z] == Terrain.DownSlopeTile)
            {
                // first try to roll it down a slope
                queue.Enqueue(new Coord(x, y, z - 1));
            }
            foreach (int i in order)
            {
                // add all eight neighbors to the queue
                var (dx, dy, dz) = Movement.Directions8[i];
                queue.Enqueue(new Coord(x + dx, y + dy, z + dz));
            }
            HashSet<Coord> tried = new HashSet<Coord>();
            while (queue.Count > 0)
            {
                Coord c = queue.Dequeue();
                tried.Add(c);
                if (Terrains[c.X, c.Y, c.Z].Solid)
                {
                    continue;
                }
                if (Creatures[c] != null)
                {
                    if (Terrains[c.X, c.Y, c.Z] == Terrain.DownSlopeTile)
                    {
                        Coord d = new Coord(c.X, c.Y, c.Z - 1);
                        if (!tried.Contains(d))
                        {
                            queue.Enqueue(d);
                        }
                    }
                    foreach (int i in order)
                    {
                        var (dx, dy, dz) = Movement.Directions8[i];
                        dx += c.X;
                        dy += c.Y;
                        dz += c.Z;
                        Coord d = new Coord(dx, dy, dz);
                        if (!tried.Contains(d) && Tiles.QuickDistance(x, y, z, dx, dy, dz) <= MaxDistance)
                        {
                            queue.Enqueue(d);
                        }
                    }
                    continue;
                }
                Game.World.Events.Publish(new SensoryEvent($"{Describe(capitalized: true)} got displaced to {c.X} {c.Y} {c.Z}.", c.X, c.Y, c.Z));
                Place(c.X, c.Y, c.Z);
                return;
            }
            Status.PushMessage($"{Describe()} was crushed by the crowd!");
            Destroy();
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
                Item.SpawnCorpse(Species).Place(x, y, z);
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
                    menu.MenuTop.Add($"Working on {t.Describe(article: false)} at {t.X} {t.Y} {t.Z}");
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
                    ControlContext.Cursor.Remove();
                };
            menu.KeyMap[Keys.Tab] = NextMinion;
            menu.KeyMap[Keys.U] = Commands.ShowStructures;
            menu.KeyMap[Keys.M] = Commands.ShowMinions;
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
                    ControlContext.Set(new MenuCameraControls((Creature)minions[0]));
                    Game.Camera.CenterOnSelection();
                }
                else
                {
                    ControlContext.Set(new MenuCameraControls(Player));
                    Game.Camera.CenterOnSelection();
                }
            }
            else if (TryComponent<Minion>()==null)
            {
                ControlContext.Set(new MenuCameraControls(Player));
                Game.Camera.CenterOnSelection();
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
                    //ControlContext.Set(new MenuChoiceControls(Player));
                    ControlContext.Set(new MenuCameraControls(Player));
                    Game.Camera.CenterOnSelection();
                }
                else
                {
                    //ControlContext.Set(new MenuChoiceControls((Creature)minions[n+1]));

                    ControlContext.Set(new MenuCameraControls((Creature)minions[n + 1]));
                    Game.Camera.CenterOnSelection();
                }
            }
        }

        public override char GetCalculatedSymbol()
        {
            if (TypeName == "Zombie" && Species != "Human")
            {
                return Hecatomb.Species.Types[Species].Symbol;
            }
            return base.GetCalculatedSymbol();
        }

        public override string GetDisplayName()
        {
            string name = base.GetDisplayName();
            if (Species != TypeName && Species != "Human")
            {
                name = Hecatomb.Species.Types[Species].Name + " " + name;
            }
            var defend = TryComponent<Defender>();
            int wounds = (defend == null) ? 0 : defend.Wounds;
            var decay = TryComponent<Decaying>();
            double rotten = (decay == null) ? 1.0 : decay.GetFraction();
            if (wounds >= 6)
            {
                return ("severely wounded " + name);
            }
            else if (rotten < 0.25)
            {
                return ("severely rotted " + name);
            }
            else if (wounds >= 4)
            {
                return ("wounded " + name);
            }
            else if (rotten < 0.5)
            {
                return ("rotted " + name);
            }
            else if (wounds >= 2)
            {
                return ("slightly wounded " + name);
            }
            else if (rotten < 0.75)
            {
                return ("slightly rotted " + name);
            }
            return name;
        }

        public static Coord? FindPlace(int x, int y, int z, int max = 5, int min = 0, bool groundLevel = true)
        {
            return Tiles.NearbyTile(x, y, z, max: max, min: min, valid: (fx, fy, fz) => { return (Creatures[fx, fy, fz] == null); });
        }

    }
}
