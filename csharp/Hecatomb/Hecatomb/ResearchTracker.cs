using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{

    class ResearchTracker : StateTracker
    {
        public List<string> Researched;
        public ResearchTracker() : base()
        {
            Researched = new List<string>();
        }

        public override void Activate()
        {
            base.Activate();
        }
    }

    public interface IResearchable
    {
        
    }
}
