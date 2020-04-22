using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class HecatombOptions
    {
        public static bool Visible;
        public static bool Explored;


        static HecatombOptions()
        {
            Visible = false;
            //Visible = true;
            Explored = false;
            //Explored = true;
        }
    }
}
