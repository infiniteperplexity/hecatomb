using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Hecatomb8
{
    using static Research;
    public class ResearchHandler : StateHandler, IDisplayInfo

    {
        public List<Research> Researched;
        public ResearchHandler() : base()
        {
            Researched = new List<Research>();
        }

        public void BuildInfoDisplay(InfoDisplayControls info)
        {
            var Commands = InterfaceState.Commands!;
            info.KeyMap[Keys.Z] = Commands.ChooseSpell;
            info.KeyMap[Keys.J] = Commands.ChooseTask;
            info.KeyMap[Keys.L] = Commands.ShowLog;
            info.KeyMap[Keys.V] = Commands.ShowAchievements;
        }

        public void FinishInfoDisplay(InfoDisplayControls info)
        {
            info.InfoTop.Add("Researched:");
            foreach (var research in Researched)
            {
                info.InfoTop.Add("- " + research.Name);
            }
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
            if (Structure.ListStructureTypes().Contains(typeof(GuardPost)))
            {
                modifier += 2;
            }
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
