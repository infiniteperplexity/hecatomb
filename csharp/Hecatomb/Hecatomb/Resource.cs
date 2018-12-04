using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hecatomb
{
    public class Resource : FlyWeight<Resource>
    {
        public string Name;
        public char Symbol;
        public string FG;
        public string BG;

        public Resource(
            string type = "",
            string name = "",
            char symbol = ' ',
            string fg = "white",
            string bg = null
        ) : base(type)
        {
            Name = name;
            Symbol = symbol;
            FG = fg;
            BG = bg;
        }

        public static string Format(ValueTuple<string, int> vt)
        {
            var (s, i) = vt;
            return (i + " " + Resource.Types[s].Name);
        }
        public static string Format(ValueTuple<string, int>[] vts)
        {
            if (vts.Length==0)
            {
                return "";
            }
            string s = Format(vts[0]);
            for(int i=1; i<vts.Length; i++)
            {
                s += (", " + Format(vts[i]));
            }
            return s;
        }
        public static string Format(List<ValueTuple<string, int>> vts)
        {
            if (vts.Count==0)
            {
                return "";
            }
            string s = Format(vts[0]);
            for (int i = 1; i < vts.Count; i++)
            {
                s += (", " + Format(vts[i]));
            }
            return s;
        }
        public static string Format(Dictionary<string, int> d)
        {
            if (d.Count==0)
            {
                return "";
            }
            List<string> list = d.Keys.ToList();
            string s = Format((list[0], d[list[0]]));
            for (int i = 1; i < list.Count; i++)
            {
                s += (", " + Format((list[i], d[list[i]])));
            }
            return s;
        }

        public static readonly Resource Rock = new Resource(
            type: "Rock",
            name: "rock",
            symbol: '\u2022',
            fg: "gray"
        );

        public static readonly Resource Flint = new Resource(
            type: "Flint",
            name: "flint",
            symbol: '\u2022',
            fg: "#CCCCCC"
        );

        public static readonly Resource Coal = new Resource(
            type: "Coal",
            name: "coal",
            symbol: '\u2022',
            fg: "#222222"
        );

        public static readonly Resource Wood = new Resource(
            type: "Wood",
            name: "wood",
            symbol: '=',
            fg: "brown"
        );


    }


}
