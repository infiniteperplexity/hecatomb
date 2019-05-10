using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Hecatomb
{
    public class ColoredText
    {
        public string Text;
        public SortedList<int, string> Colors;
        private static Regex capture = new Regex("\\{([^}]*)\\}");
        private static char[] braces = new char[] { '{', '}' };

        public ColoredText(string s)
        {
            Colors = new SortedList<int, string>();
            string txt = s;
            Match m = null;
            while (true)
            {
                m = capture.Match(txt);
                if (!m.Success)
                {
                    break;
                }
                Colors[m.Index] = m.Value.Trim(braces);
                txt = txt.Remove(m.Index, m.Value.Length);
            }
            Text = txt;
        }

        public ColoredText(string s, string c) : this(s)
        {
            Colors[0] = c;
        }

        public int Length
        {
            get { return Text.Length; }
            set { }
        }

        public static implicit operator string(ColoredText ct)
        {
            return ct.Text;
        }

        public static implicit operator ColoredText(string s)
        {
            return new ColoredText(s);
        }

    }
}
