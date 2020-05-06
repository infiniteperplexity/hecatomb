using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Necromancer : Creature
    {
        public Necromancer()
        {
            _name = "necromancer";
            _symbol = '@';
            _fg = "magenta";
            Components.Add(new SpellCaster()
                {
                    Spells = new List<Type>() {
                        typeof(RaiseZombieSpell),
                        typeof(SacrificeMinionSpell),
                        typeof(CondenseEctoplasmSpell),
                        typeof(SiphonFleshSpell),
                        typeof(ShadowHopSpell),
                        typeof(DebugSpell)
                    }
                }
            );
            GetPrespawnComponent<Actor>().Team = Team.Friendly;
            GetPrespawnComponent<Defender>().Evasion = 1;
        }
    }
}