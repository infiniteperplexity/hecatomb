﻿using System;
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

    public class ShadowHopSpell : Spell
    {
        public ShadowHopSpell()
        {
            MenuName = "shadow hop";
            cost = 10;
            Researches = new[] { "ShadowHop" };
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
                Cast();
            }
        }
        public override void Cast()
        {
            CommandLogger.LogCommand(command: "ShadowHop");
            var (x, y, z) = Caster;
            ParticleEmitter emitter1 = new ParticleEmitter();
            emitter1.Place(x, y, z);
            var m = Caster.GetComponent<Movement>();
            Coord? cc = Tiles.NearbyTile(x, y, z, max: 8, min: 3, valid: (int x1, int y1, int z1) => (m.CanStand(x1, y1, z1)));
            if (cc != null)
            {
                Coord c = (Coord)cc;
                Caster.GetComponent<Movement>().StepTo(c.X, c.Y, c.Z);
                Caster.GetComponent<Actor>().Spend();
                Game.Camera.Center(c.X, c.Y, c.Z);
                ControlContext.Reset();
                ParticleEmitter emitter2 = new ParticleEmitter();
                emitter2.Place(c.X, c.Y, c.Z);
                base.Cast();
                Game.InfoPanel.PushMessage("You vanish and reappear nearby.");
            }
            else
            {
                Game.InfoPanel.PushMessage("The spell fizzles.");
            }
        }
    }
}
