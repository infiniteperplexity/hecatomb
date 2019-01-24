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

    public class CondenseEctoplasmSpell : Spell
    {
        public CondenseEctoplasmSpell()
        {
            MenuName = "condense ectoplasm";
            cost = 20;
            Researches = new[] { "CondenseEctoplasm" };
            Structures = new[] { "Sanctum"};
        }

        public override void Cast()
        {
            var (x, y, z) = Caster;
            ParticleEmitter emitter = new ParticleEmitter();
            emitter.Place(x, y, z);
            Item.PlaceNewResource("Ectoplasm", 1, x, y, z);
            base.Cast();
        }
    }
}
