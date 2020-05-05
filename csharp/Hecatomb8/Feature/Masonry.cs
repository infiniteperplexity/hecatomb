using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Masonry : Feature
    {
		public Masonry()
		{
			_name = "masonry";
			alwaysPlural = true;
			_symbol = ' ';
			Components.Add(new Harvestable());
			Components.Add(new Fixture() { 
				Labor = 1,
				RequiresStructures = new Type[] {typeof(Workshop)}
			});
		}
    }
}