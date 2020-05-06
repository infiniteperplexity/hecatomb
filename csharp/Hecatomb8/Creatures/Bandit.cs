using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Bandit : Creature
    {
        public Bandit()
        {
            _name = "bandit";
            _fg = "brown";
            _symbol = '@';
            GetPrespawnComponent<Actor>().Team = Team.Evil;
        }
    }
}
