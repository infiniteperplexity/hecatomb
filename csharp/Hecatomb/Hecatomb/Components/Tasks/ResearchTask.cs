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
    class ResearchTask : System.Threading.Tasks.Task
    {
        EntityField<Structure> Structure;
        public string MyResearch;

        //public override void Start()
        //{
        //}
        // not quite certain how I want to do this...

        //public ResearchTask()
        public override bool CanAssign(Creature c)
        {
            if (Labor<LaborCost)
            {
                return false;
            }
            return base.CanAssign(c);
        }
        public override void Work()
        {
            Unassign();
            //Structure.Researching = 
        }
        public override void Finish()
        {

        }
    }
}
