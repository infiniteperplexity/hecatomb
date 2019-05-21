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

        public override void Destroy()
        {
            int x = X;
            int y = Y;
            int z = Z;
            base.Destroy();
            Item.SpawnCorpse().Place(x, y, z);
        }

        public void BuildMenu(MenuChoiceControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            menu.Header = "Creature: " + Describe();
            menu.Choices = new List<IMenuListable>();
            Highlight = "lime green";
        }
        public void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.Insert(2, "Tab) Next minion.");
            if (TryComponent<Minion>() != null)
            {
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
            menu.KeyMap[Keys.Escape] =
                () =>
                {
                    Highlight = null;
                    Game.Controls.Reset();
                };
            menu.KeyMap[Keys.Tab] = NextMinion;
        }

        public void NextMinion()
        {
            var minions = GetState<TaskHandler>().Minions;
            if (this==Player)
            {
                if (minions.Count>0)
                {
                    Game.Controls.Set(new MenuChoiceControls((Creature) minions[0]));
                }
            }
            else if (TryComponent<Minion>()==null)
            {
                Game.Controls.Set(new MenuChoiceControls(Player));
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
                    Game.Controls.Set(new MenuChoiceControls(Player));
                }
                else
                {
                    Game.Controls.Set(new MenuChoiceControls((Creature)minions[n+1]));
                }
            }
        }
       
    }
}
