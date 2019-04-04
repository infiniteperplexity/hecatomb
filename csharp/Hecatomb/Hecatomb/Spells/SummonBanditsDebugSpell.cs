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

    public class SummonBanditsDebugSpell : Spell
    {
        public SummonBanditsDebugSpell()
        {
            MenuName = "summon bandits (debug)";
            cost = 0;
        }

        public override void ChooseFromMenu()
        {
            Cast();
            Foreground.Splash(new List<ColoredText>{
                "Testing",
                "Testing",
                "Testing"
            });
            GetState<HumanTracker>().BanditAttack(debugCloser: true);

        }
    }
}
