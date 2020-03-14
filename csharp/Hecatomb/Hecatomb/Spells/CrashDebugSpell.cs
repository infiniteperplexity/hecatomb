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

    public class CrashDebugSpell : Spell
    {
        public CrashDebugSpell()
        {
            MenuName = "throw an exception (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            throw new Exception("debugging exception");
        }
    }
}
