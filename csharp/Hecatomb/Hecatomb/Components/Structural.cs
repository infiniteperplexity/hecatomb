using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    class Structural : Component
    {
        private int StructureEID;
        public StructureEntity Structure
        {
            get
            {
                if (StructureEID==-1)
                {
                    return null;
                }
                else
                {
                    return (StructureEntity) Entities[StructureEID];
                }
            }
            set
            {
                if (value==null)
                {
                    StructureEID = -1;
                }
                else
                {
                    StructureEID = value.EID;
                }
            }
        }
    }
}
