using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class HecatombOptions
    {
        public static bool Visible;
        public static bool Explored;
        public static bool NoStartupScreen;
        public static bool NoTutorial;
        public static bool NoIngredients;
        public static int WorkBonus;
        public static bool Invincible;
        public static bool ShowDebugSpells;
        public static bool ShowAllSpells;
        public static bool IgnoreHardness;
        public static bool NoSpiders;
        public static bool NoManaCost;
        public static bool NoisyErrors;


        static HecatombOptions()
        {
            Visible = false;
            //Visible = true;
            Explored = false;
            //Explored = true;
            NoStartupScreen = false;
            //NoStartupScreen = true;
            //NoTutorial = false;
            NoTutorial = true;
            NoIngredients = false;
            NoIngredients = true;
            WorkBonus = 0;
            WorkBonus = 2;
            NoManaCost = false;
            NoManaCost = true;
            Invincible = false;
            NoisyErrors = true;
            //NoisyErrors = false;
        }
    }
}
