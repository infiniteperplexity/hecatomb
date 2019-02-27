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
        public bool NoTutorial;
        public bool NoStartupScreen;
        public bool HumanAttacks;
        public int WorkBonus;
        public bool AllSpells;
        public bool NoManaCost;

        public HecatombOptions()
        {
            Explored = false;
            Visible = false;
            NoIngredients = false;
            NoIngredients = true;
            NoTutorial = false;
            NoTutorial = true;
            NoStartupScreen = false;
            NoStartupScreen = true;
            HumanAttacks = false;
            AllSpells = false;
            NoManaCost = false;
            NoManaCost = true;
            WorkBonus = 0;
            WorkBonus = 5;
            
        }
    }
    
}
