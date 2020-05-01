using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static Research;
    public class ResearchHandler : StateHandler /*, IListPopulater*/

    {
        public List<Research> Researched;
        public ResearchHandler() : base()
        {
            Researched = new List<Research>();
        }

        public List<ColoredText> GetLines()
        {
            var list = new List<ColoredText> { "Researched:" };
            foreach (var research in Researched)
            {
                list.Add("- " + research.Name);
            }
            return list;
        }
        public int GetToolHardness()
        {
            if (Researched.Contains(AlloyTools))
            {
                return 4;
            }
            else if (Researched.Contains(SteelTools))
            {
                return 3;
            }
            else if (Researched.Contains(BronzeTools))
            {
                return 2;
            }
            else if (Researched.Contains(FlintTools))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public int GetMinionDamage()
        {
            int modifier = 0;
            if (Researched.Contains(BoneWeapons))
            {
                modifier += 2;
            }
            return modifier;
        }

        public int GetMinionAccuracy()
        {
            return 0;
        }

        public int GetMinionEvasion()
        {
            int modifier = 0;
            //if (Structure.ListStructureTypes().Contains(GuardPost))
            //{
            //    modifier += 2;
            //}
            return modifier;
        }

        public int GetMinionToughness()
        {
            return 0;
        }

        public int GetMinionArmor()
        {
            return 0;
        }
    }
}
