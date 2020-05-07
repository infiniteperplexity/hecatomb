using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Door : Feature
    {
        public Door()
        {
            _name = "door";
            _fg = "brown";
            _symbol = '\u25A7';
            Solid = true;
            Components.Add(new Fixture()
            {
                Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Wood, 2 } },
                RequiresStructures = new Type[] { typeof(Workshop) }
            });
            Components.Add(new Defender() { Toughness = 9, Evasion = -10 });
        }
    }
}
