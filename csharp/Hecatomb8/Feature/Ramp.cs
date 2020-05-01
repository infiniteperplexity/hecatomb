using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Ramp : Feature
    {
        public Ramp()
        {
            _name = "ramp";
            _fg = "WALLFG";
            _symbol = '^';
            Components.Add(new Fixture()
            {
                Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Rock, 2 } },
                RequiresStructures = new Type[] { typeof(Workshop) }
            });
        }
    }
}
