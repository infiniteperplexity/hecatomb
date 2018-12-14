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

        public HecatombOptions()
        {
            Explored = false;
            Visible = false;
            NoIngredients = false;
        }
    }
    
}
