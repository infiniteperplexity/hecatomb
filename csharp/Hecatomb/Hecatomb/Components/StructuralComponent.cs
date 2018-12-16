using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    using static HecatombAliases;

    class StructuralComponent : Component
    {
        public TileEntityField<Structure> Structure;
        public StructuralComponent(): base()
        {
            Structure = new TileEntityField<Structure>();
        }
    }
}
