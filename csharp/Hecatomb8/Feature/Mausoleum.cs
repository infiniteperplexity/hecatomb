using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Mausoleum : Feature
    {
        public Mausoleum()
        {
            _name = "mausoleum";
            _fg = "WALLFG";
            _symbol = '\u271F';
            Components.Add(new Harvestable() { Yields = new JsonArrayDictionary<Resource, float>() { [Resource.Gold] = 1, [Resource.Corpse] = 1 } });
        }
    }
}
