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

    class LongShadowSpell : Spell
    {
        public LongShadowSpell()
        {
            MenuName = "long shadow";
            cost = 10;
            Researches = new[] { "LongShadow" };
            Structures = new[] { "Sanctum" };
        }
    }
}
