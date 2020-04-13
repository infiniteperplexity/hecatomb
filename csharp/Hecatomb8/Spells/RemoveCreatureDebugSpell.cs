using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class RemoveCreatureDebugSpell : Spell, ISelectsTile
    {
        public RemoveCreatureDebugSpell()
        {
            MenuName = "remove creature (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            base.ChooseFromMenu();
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                var c = new SelectTileControls(this);
                c.MenuSelectable = false;
                c.SelectedMenuCommand = "Spells";
                ControlContext.Set(c);
            }
        }

        public void SelectTile(Coord c)
        {
            Creature cr = OldGame.World.Creatures[c.X, c.Y, c.Z];
            if (cr != null && (OldGame.World.Explored.Contains(c) || Options.Explored))
            {
                cr.Despawn() ;
            }
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Creature cr = Creatures[x, y, z];
            if (!OldGame.World.Explored.Contains(c) && !Options.Explored)
            {
                OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (cr != null)
            {
                if (cr == Caster)
                {
                    OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{orange}" + String.Format("Cannot target yourself") };
                }
                else if (cr.GetComponent<Actor>().Team == Caster.GetComponent<Actor>().Team)
                {
                    OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel your own flesh to heal {0}", cr.Describe()) };
                }
                else
                {
                    OldGame.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel {0} to heal your own flesh.", cr.Describe()) };
                }
            }
        }
    }
}
