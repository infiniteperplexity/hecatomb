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

    public class SiphonFleshSpell : Spell, ISelectsTile
    {
        public SiphonFleshSpell()
        {
            MenuName = "siphon flesh";
            _cost = 5;
            RequiresResearch = new[] { Research.SiphonFlesh };
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
                c.InfoMiddle = new List<ColoredText>() { "{green}Siphon flesh." };
                InterfaceState.SetControls(c);
            }
        }

        public void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "SiphonFlesh", x: c.X, y: c.Y, z: c.Z);
            Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);

            if (cr != null && (Explored.Contains(c) || HecatombOptions.Explored))
            {
                if (cr == Caster)
                {
                    // can't target caster
                }
                else if (cr.GetComponent<Actor>().Team == Caster!.GetComponent<Actor>().Team)
                {
                    // heal ally at expense of caster
                    // can we make this heal two points of damage and 200 points of rot for life?
                    Defender d1 = cr.GetComponent<Defender>();
                    Defender d2 = Caster.GetComponent<Defender>();
                    //Decaying decay = cr.TryComponent<Decaying>();
                    int siphon = Math.Min(d1.Wounds, (14 - d2.Wounds) * 2);
                    if (siphon > 0)
                    {
                        d1.Wounds -= siphon;
                        d2.Wounds += (int)Math.Ceiling(((double)siphon) / 2.0);
                    }
                    if (cr is Zombie)
                    {
                        var zombie = (Zombie)cr;
                        zombie.Decay = Math.Min(zombie.MaxDecay, zombie.Decay + 500);
                    }
                    //else if (decay != null)
                    //{
                    //    // I think let's hold off on this for now
                    //}
                    if (siphon > 0)
                    {
                        PushMessage("You siphon your own flesh and blood to heal your minion.");
                        ParticleEmitter emitter1 = new ParticleEmitter();
                        emitter1.Place((int)Caster.X!, (int)Caster.Y!, (int)Caster.Z!);
                        ParticleEmitter emitter2 = new ParticleEmitter();
                        emitter2.Place(c.X, c.Y, c.Z);
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
                        cr.GetComponent<Actor>().ProvokeAgainst(Caster);
                        int heal = Math.Min(d2.Wounds, 20 - d1.Wounds);
                        d1.Wounds += heal;
                        d2.Wounds -= heal;
                        PushMessage($"You siphon flesh and blood from {cr.Describe()} to mend your wounds.");
                        ParticleEmitter emitter1 = new ParticleEmitter();
                        emitter1.Place((int)Caster!.X!, (int)Caster.Y!, (int)Caster.Z!);
                        ParticleEmitter emitter2 = new ParticleEmitter();
                        emitter2.Place(c.X, c.Y, c.Z);
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
            Creature? cr = Creatures.GetWithBoundsChecked(x, y, z);
            var controls = InterfaceState.Controls;
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                controls.InfoMiddle = new List<ColoredText>() { "{orange}Unexplored tile." };
            }
            else if (cr != null)
            {
                if (cr == Caster)
                {
                    controls.InfoMiddle = new List<ColoredText>() { "{orange}" + String.Format("Cannot target yourself") };
                }
                else if (cr.GetComponent<Actor>().Team == Caster!.GetComponent<Actor>().Team)
                {
                    controls.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel your own flesh to heal {0}", cr.Describe()) };
                }
                else
                {
                    controls.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Shrivel {0} to heal your own flesh.", cr.Describe()) };
                }
            }
        }
    }
}
