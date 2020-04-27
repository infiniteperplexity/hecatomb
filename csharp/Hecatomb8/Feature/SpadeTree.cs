using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class SpadeTree : Feature
    {
        public SpadeTree()
        {
            _name = "tree";
            _fg = "#669933";
            _symbol = '\u2660';
            Components.Add(new Harvestable() { Yields = new Dictionary<Resource, float>() { [Resource.Wood] = 1 } });
        }
    }
}
