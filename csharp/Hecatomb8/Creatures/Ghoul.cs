using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    public class Ghoul : Creature
    {
        public Ghoul()
        {
            _symbol = 'z';
            _name = "ghoul";
            _fg = "orange";
            Components.Add(new Inventory());
            GetPrespawnComponent<Actor>().Team = Team.Berserk;
            GetPrespawnComponent<Attacker>().Damage = 1;
            GetPrespawnComponent<Attacker>().Accuracy = 1;
            GetPrespawnComponent<Defender>().Evasion = 1;
            Species = Species.Undead;
        }
    }
}