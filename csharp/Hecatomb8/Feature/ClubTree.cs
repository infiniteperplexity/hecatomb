using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class ClubTree : Feature
    {
        public ClubTree()
        {
            _name = "tree";
            _fg = "#44AA33";
            _symbol = '\u2663';
            Components.Add(new Harvestable() { Yields = new JsonArrayDictionary<Resource, float> () { [Resource.Wood] = 1} });
        }
    }
}
