using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class SacrificeMinionSpell : Spell, ISelectsTile
    {
        public SacrificeMinionSpell()
        {
            MenuName = "sacrifice minion";
            _cost = 1;
            RequiresStructures = new[] { typeof(Sanctum) };
        }

        public override void ChooseFromMenu()
        {
            base.ChooseFromMenu();
            if (Cost > Component!.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                var c = new SelectTileControls(this);
                c.MenuCommandsSelectable = false;
                c.SelectedMenuCommand = "Spells";
                InterfaceState.SetControls(c);
            }
        }

        public void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "SacrificeMinion", x: c.X, y: c.Y, z: c.Z);
            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);

            if (cr != null && (Explored.Contains(c) || HecatombOptions.Explored))
            {
                if (cr != Caster && cr.GetComponent<Actor>().Team == Caster!.GetComponent<Actor>().Team)
                {
                    PushMessage("You withdraw magical power from the minion.");
                    ParticleEmitter emitter1 = new ParticleEmitter();
                    emitter1.Place((int)Caster!.X!, (int)Caster!.Y!, (int)Caster!.Z!);
                    ParticleEmitter emitter2 = new ParticleEmitter();
                    emitter2.Place((int)cr.X!, (int)cr.Y!, (int)cr.Z!);
                    cr.Destroy();
                    Caster.GetComponent<SpellCaster>().Sanity = Caster.GetComponent<SpellCaster>().MaxSanity;
                }
            }
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Creature? cr = Creatures.GetWithBoundsChecked(x, y, z);
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                InterfaceState.Controls.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (cr != null)
            {
                if (cr != Caster && cr.GetComponent<Actor>().Team == Caster!.GetComponent<Actor>().Team)
                {
                    InterfaceState.Controls.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Sacrifice {0} to restore sanity.", cr.Describe()) };
                }
            }
        }
    }
}
