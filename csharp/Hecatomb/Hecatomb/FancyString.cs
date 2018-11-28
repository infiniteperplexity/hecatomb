using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    class FancyString
    {
        private string value;
        public FancyString(string s)
        {
            value = s;
        }

        public static implicit operator FancyString(string s)
        {
            return new FancyString(s);
        }

        public static implicit operator string(FancyString fs)
        {
            return fs.value;
        }
    }
}
