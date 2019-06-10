﻿using System;
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

        public static ColoredText operator +(ColoredText ct1, ColoredText ct2)
        {
            string text = ct1.Text + ct2.Text;
            var colors = new SortedList<int, string>(ct1.Colors);
            if (!ct2.Colors.ContainsKey(0))
            {
                colors[ct1.Text.Length + 1] = "white";
            }
            int length = ct1.Text.Length;
            foreach (var key in ct2.Colors.Keys)
            {
                colors[length + key] = ct2.Colors[key];
            }
            var ct = new ColoredText("");
            ct.Text = text;
            ct.Colors = colors;
            return ct;
        }

    }
}
