using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class HecatombOptions
    {
        public static bool Visible;
        public static bool Explored;
        public static bool NoTutorial;
        public static bool NoIngredients;
        public static int WorkBonus;
        public static bool Invincible;


        static HecatombOptions()
        {
            Visible = false;
            //Visible = true;
            Explored = false;
            //Explored = true;
            NoTutorial = false;
            //NoTutorial = true;
            NoIngredients = false;
            //NoIngredients = true;
            WorkBonus = 0;
            Invincible = false;
        }
    }
}
