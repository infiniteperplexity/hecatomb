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

    public class SacrificeMinionSpell : Spell, ISelectsTile
    {
        public SacrificeMinionSpell()
        {
            MenuName = "sacrifice minion";
            cost = 1;
            Structures = new[] { "Sanctum" };
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
            CommandLogger.LogCommand(command: "SacrificeMinion", x: c.X, y: c.Y, z: c.Z);
            Creature cr = Game.World.Creatures[c.X, c.Y, c.Z];

            if (cr != null && (Game.World.Explored.Contains(c) || Options.Explored))
            {
                if (cr != Caster && cr.GetComponent<Actor>().Team == Caster.GetComponent<Actor>().Team)
                {
                    Game.InfoPanel.PushMessage("You withdraw magical power from the minion.");
                    ParticleEmitter emitter1 = new ParticleEmitter();
                    emitter1.Place(Caster.X, Caster.Y, Caster.Z);
                    ParticleEmitter emitter2 = new ParticleEmitter();
                    emitter2.Place(cr.X, cr.Y, cr.Z);
                    cr.Destroy();
                    Caster.GetComponent<SpellCaster>().Sanity = Caster.GetComponent<SpellCaster>().GetCalculatedMaxSanity();
                }
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
                if (cr != Caster && cr.GetComponent<Actor>().Team == Caster.GetComponent<Actor>().Team)
                {
                    Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Sacrifice {0} to restore sanity.", cr.Describe()) };
                }
            }
        }
    }
}
