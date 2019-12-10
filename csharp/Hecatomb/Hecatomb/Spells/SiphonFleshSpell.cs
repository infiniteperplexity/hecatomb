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

    public class SiphonFleshSpell : Spell, ISelectsTile
    {
        public SiphonFleshSpell()
        {
            MenuName = "siphon flesh";
            cost = 5;
            Researches = new[] { "SiphonFlesh" };
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
            Creature cr = Game.World.Creatures[c.X, c.Y, c.Z];

            if (cr!=null && (Game.World.Explored.Contains(c) || Options.Explored))
            {
                if (cr == Caster)
                {
                    // can't target caster
                }
                else if (cr.GetComponent<Actor>().Team == Caster.GetComponent<Actor>().Team)
                {
                    // heal ally at expense of caster
                    // can we make this heal two points of damage and 200 points of rot for life?
                    Defender d1 = cr.GetComponent<Defender>();
                    Defender d2 = Caster.GetComponent<Defender>();
                    Decaying decay = cr.TryComponent<Decaying>();
                    int siphon = Math.Min(d1.Wounds, (14 - d2.Wounds)*2);
                    if (siphon > 0)
                    {
                        d1.Wounds -= siphon;
                        d2.Wounds += (int)Math.Ceiling(((double)siphon) / 2.0);
                    }
                    else if (decay != null)
                    {
                        // I think let's hold off on this for now
                    }
                    if (siphon > 0)
                    {
                        Game.InfoPanel.PushMessage("You siphon your own flesh and blood to heal your minion.");
                        ParticleEmitter emitter1 = new ParticleEmitter();
                        emitter1.Place(Caster.X, Caster.Y, Caster.Z);
                        ParticleEmitter emitter2 = new ParticleEmitter();
                        emitter2.Place(cr.X, cr.Y, cr.Z);
                        base.Cast();
                        d2.ResolveWounds();
                    }
                }
                else
                {
                    // heal caster at expense of target
                    Defender d1 = cr.GetComponent<Defender>();
                    Defender d2 = Caster.GetComponent<Defender>();
                 
                    if (d2.Wounds > 0)
                    {
                        int heal = Math.Min(d2.Wounds, 20 - d1.Wounds);
                        d1.Wounds += heal;
                        d2.Wounds -= heal;
                        Game.InfoPanel.PushMessage($"You siphon flesh and blood from {cr.Describe()} to mend your wounds.");
                        ParticleEmitter emitter1 = new ParticleEmitter();
                        emitter1.Place(Caster.X, Caster.Y, Caster.Z);
                        ParticleEmitter emitter2 = new ParticleEmitter();
                        emitter2.Place(cr.X, cr.Y, cr.Z);
                        base.Cast();
                        d1.ResolveWounds();
                    }   
                }
            }
            else
            {
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
                if (cr==Caster)
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
