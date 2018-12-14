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
    }
}
