using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Grave : Feature
    {
        public Grave()
        {
            _name = "grave";
            _fg = "WALLFG";
            _symbol = '\u271D';
            Components.Add(new Harvestable() { Yields = new Dictionary<Resource, float>() { [Resource.Corpse] = 1 } });
        }
    }
}
