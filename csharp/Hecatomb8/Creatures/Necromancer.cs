using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Necromancer : Creature
    {
        public Cover coverTest = Cover.Water;

        public Necromancer()
        {
            _name = "necromancer";
            _symbol = '@';
            _fg = "magenta";
            Components.Add(new SpellCaster()
                {
                    Spells = new List<Type>() {
                        typeof(RaiseZombieSpell)
                    }
                }
            );
        }
    }
}
