using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class FootprintManager : StateHandler
    {
        public HashSet<Coord> Footprints;
        public FootprintManager()
        {
            Footprints = new HashSet<Coord>();
        }
    }
}
