using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hecatomb
{
    public class ResearchHandler : StateHandler
    {
        public List<string> Researched;
        public ResearchHandler() : base()
        {
            Researched = new List<string>();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public int GetToolHardness()
        {
            if (Researched.Contains("AlloyTools"))
            {
                return 4;
            }
            else if (Researched.Contains("SteelTools"))
            {
                return 3;
            }
            else if (Researched.Contains("BronzeTools"))
            {
                return 2;
            }
            else if (Researched.Contains("FlintTools"))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
