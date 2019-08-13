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

    public class LongShadowSpell : Spell
    {
        public LongShadowSpell()
        {
            MenuName = "long shadow";
            cost = 10;
            Researches = new[] { "LongShadow" };
            Structures = new[] { "Sanctum" };
        }

        public override void Cast()
        {
            var (x, y, z) = Caster;
            ParticleEmitter emitter1 = new ParticleEmitter();
            emitter1.Place(x, y, z);
            var m = Caster.GetComponent<Movement>();
            Coord c = new Coord(-1, -1, -1);
            int tries = 0;
            int maxTries = 50;
            while (c.X == -1)
            {
                c = Tiles.NearbyTile(x, y, z, max: 8, min: 3, valid: (int x1, int y1, int z1) => (m.CanStand(x1, y1, z1)));
                tries += 1;
                if (tries > maxTries)
                {
                    break;
                }
            }
            if (c.X != -1)
            {
                Debug.WriteLine("This is a thing happening");
                Caster.GetComponent<Movement>().StepTo(c.X, c.Y, c.Z);
                Caster.GetComponent<Actor>().Spend();
                Game.Camera.Center(c.X, c.Y, c.Z);
                Controls.Reset();
                ParticleEmitter emitter2 = new ParticleEmitter();
                emitter2.Place(c.X, c.Y, c.Z);
                base.Cast();
            }
        }
    }
}
