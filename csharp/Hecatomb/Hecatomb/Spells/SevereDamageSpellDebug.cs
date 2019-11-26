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

    public class SevereDamageDebugSpell : Spell, ISelectsTile
    {
        public SevereDamageDebugSpell()
        {
            MenuName = "severe damage (debug)";
            cost = 0;
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
                ControlContext.Set(new SelectTileControls(this));
            }
        }

        public void SelectTile(Coord c)
        {
            Creature cr = Game.World.Creatures[c.X, c.Y, c.Z];
            Feature f = Game.World.Features[c.X, c.Y, c.Z];
            if (cr != null && (Game.World.Explored.Contains(c) || Options.Explored))
            {
                cr.GetComponent<Defender>().Wounds = 6;
            }
            else if (f != null && f.TryComponent<Defender>() != null)
            {
                f.GetComponent<Defender>().Wounds = 6;
                // not sure what to do, if there's no message scroll
            }
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Creature cr = Creatures[x, y, z];
            if (!Game.World.Explored.Contains(c) && !Options.Explored)
            {
                Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (cr != null)
            {
                if (cr == Caster)
                {
                    Game.Controls.MenuMiddle = new List<ColoredText>() { "{orange}" + String.Format("Cannot target yourself") };
                }
                else if (cr.GetComponent<Actor>().Team == Caster.GetComponent<Actor>().Team)
                {
                    Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel your own flesh to heal {0}", cr.Describe()) };
                }
                else
                {
                    Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel {0} to heal your own flesh.", cr.Describe()) };
                }
            }
        }
    }
}
