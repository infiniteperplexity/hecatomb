using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class SpiderWeb : Feature
    {
		public SpiderWeb()
		{
			_name = "spider web";
			_fg = "#DDDDDD";
			_symbol = '*';
			Components.Add(new Harvestable() { Yields = new JsonArrayDictionary<Resource, float>() { [Resource.Silk] = 1 } });
		}
    }
}