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

    class CondenseEctoplasmSpell : Spell
    {
        public CondenseEctoplasmSpell()
        {
            MenuName = "condense ectoplasm";
            cost = 20;
            Researches = new[] { "CondenseEctoplasm" };
            Structures = new[] { "Sanctum"};
        }

        public override void ChooseFromMenu()
        {
            Cast();
        }

        public override void Cast()
        {
            Component.Sanity -= GetCost();
            var (x, y, z) = Caster;
            Item ectoplasm = Item.FromResources(new Dictionary<string, int>() { { "Ectoplasm", 1 } });
            ectoplasm.Place(x, y, z);
        }
    }
}
