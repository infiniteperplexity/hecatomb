using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class HecatombOptions
    {
        public bool Explored;
        public bool Visible;
        public bool NoIngredients;
        public bool Tutorial;
        public bool StartupScreen;
        public bool HumanAttacks;
        public int WorkBonus;
        public bool AllSpells;

        public HecatombOptions()
        {
            Explored = true;
            Visible = true;
            NoIngredients = true;
            Tutorial = false;
            StartupScreen = false;
            HumanAttacks = false;
            AllSpells = true;
            WorkBonus = 5;
        }
    }
    
}
