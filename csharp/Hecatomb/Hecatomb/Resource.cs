using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hecatomb
{
    public class Resource : FlyWeight
    {
        public char Symbol;
        public string FG;
        public string BG;

        public Resource(
            string type = "",
            string name = "",
            char symbol = ' ',
            string fg = "white",
            string bg = null
        ) : base(type, name)
        {
            Symbol = symbol;
            FG = fg;
            BG = bg;
        }

        public static readonly Resource Rock = new Resource(
            type: "Rock",
            name: "rock",
            symbol: '\u2022',
            fg: "gray"
        );

        public static readonly Resource Wood = new Resource(
            type: "Wood",
            name: "wood",
            symbol: '=',
            fg: "brown"
        );


    }


}
