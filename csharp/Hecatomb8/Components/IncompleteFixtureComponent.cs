using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// so I don't forget...this isn't a component that goes on a structure; rather, it's a component that links some other Entity *to* a structure
// currently used for the features that make up a structure
namespace Hecatomb
{
    using static HecatombAliases;

    class IncompleteFixtureComponent: Component
    {
        public string Makes;
        public TileEntityField<Structure> Structure;

        public IncompleteFixtureComponent()
        {
            Structure = new TileEntityField<Structure>();
        }
    }
}
