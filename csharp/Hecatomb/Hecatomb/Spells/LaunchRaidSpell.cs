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

    public class LaunchRaidDebugSpell : Spell
    {
        public LaunchRaidDebugSpell()
        {
            MenuName = "launch raid (debug)";
            cost = 0;
            ForDebugging = true;
        }

        public override void ChooseFromMenu()
        {
            Cast();
            GetState<RaidHandler>().LaunchRaid();

        }
    }
}
