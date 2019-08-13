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

    public class PoundOfFleshSpell : Spell, ISelectsTile
    {
        public PoundOfFleshSpell()
        {
            MenuName = "siphon flesh";
            cost = 5;
            Researches = new[] { "PoundOfFlesh" };
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
                Game.Controls.Set(new SelectTileControls(this));
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
                    Defender d1 = cr.GetComponent<Defender>();
                    Defender d2 = Caster.GetComponent<Defender>();
                    if (d1.Wounds > 0)
                    {
                        int heal = Math.Min(d1.Wounds, 14 - d2.Wounds);
                        Decaying decay = cr.TryComponent<Decaying>();
                        if (decay != null)
                        {
                            decay.Decay = decay.TotalDecay;
                            d1.Wounds -= heal;
                            d2.Wounds += (heal +1);
                        }
                        else
                        {
                            d1.Wounds -= heal;
                            d2.Wounds += heal;
                        }
                        
                        Game.StatusPanel.PushMessage("You siphon your own flesh and blood to heal your minion.");
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
                        int heal = Math.Min(d2.Wounds, 20 - d2.Wounds);
                        d1.Wounds += heal;
                        d2.Wounds -= heal;
                        Game.StatusPanel.PushMessage("You siphon your minion's flesh and blood to mend your own.");
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
