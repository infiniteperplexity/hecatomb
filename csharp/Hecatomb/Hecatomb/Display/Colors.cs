/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 9/18/2018
 * Time: 9:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System;

namespace Hecatomb
{
    /// <summary>
    /// Descripti
    /// </summary>
    public class Colors
    {

        private Dictionary<string, Color> colors;

        public Colors()
        {
            colors = new Dictionary<string, Color>() {
                {"white", Color.White},
                {"gray", Color.Gray},
                {"dark gray", new Color(0x55, 0x55, 0x55)},
                {"lime green", Color.LimeGreen},
                {"olive", Color.Olive},
                {"dark green", Color.DarkGreen },
                {"forest green", Color.ForestGreen},
                {"light gray", Color.LightGray},
                {"light cyan", Color.LightCyan},
                {"gainsboro", Color.Gainsboro},
                {"black", Color.Black},
                {"magenta", Color.Magenta},
                {"green", Color.LightGreen},
                {"orange", Color.Orange},
                {"yellow", Color.Yellow},
                {"cyan", Color.Cyan},
                {"brown", Color.Brown},
                {"purple", Color.Purple },
                {"pink", Color.Pink },
                {"GRASSFG", new Color(0x66, 0x88, 0x44)},
                {"GRASSBG", new Color(0x33, 0x44, 0x22)},
                {"WALLFG", new Color(0xAA, 0xAA, 0xAA)},
                {"FLOORFG", new Color(0x77, 0x77, 0x88)},
                {"BELOWFG", new Color(0x55, 0x33, 0x55)},
                {"SHADOWFG", new Color(0x33, 0x33, 0x55)},
                {"WALLBG", new Color(0x77, 0x77, 0x88)},
                {"FLOORBG", new Color(0x44, 0x55, 0x66)},
                {"BELOWBG", new Color(0x11, 0x11, 0x22)},
                {"TWOBELOWFG", new Color(0x22, 0x11, 0x22)},
                {"WATERFG", new Color(0x33, 0x88, 0xFF)},
                {"WATERBG", new Color(0x11, 0x44, 0xBB)},
                {"DARKGRASS", new Color(0x0E, 0x22, 0x11)},
                {"DARKWATER", new Color(0x05, 0x22, 0x88)}
            };

            //			PropertyInfo[] properties = typeof(Color).GetProperties();
            //			foreach(PropertyInfo property in properties)
            //			{
            //				if (property.DeclaringType.Equals(typeof(Color)))
            //				{
            //				    string Name = property.Name;
            //					string name = Name.ToLower();
            //					Debug.WriteLine(property.GetValue(typeof(Color)));
            ////					Color c = (Color) property.GetValue(typeof(Color));
            ////					colors[Name] = c;
            ////					colors[name] = c;	
            //				}
            //			}
        }

        public Color this[string s]
        {
            get
            {
                Color result;
                if (colors.TryGetValue(s, out result))
                {
                    return result;
                }
                else if (s[0] == '#')
                {
                    int r = Int32.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                    int g = Int32.Parse(s.Substring(3, 2), NumberStyles.HexNumber);
                    int b = Int32.Parse(s.Substring(5, 2), NumberStyles.HexNumber);
                    return new Color(r, g, b);
                }
                else
                {
                    return Color.Red;
                }
            }
            set { colors[s] = value; }
        }

        public static string Stringify(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        public string Interpolate(string s1, string s2, double r=0.5)
        {
            Color c1 = this[s1];
            Color c2 = this[s2];
            Color c = new Color();
            double r1 = 1 - r;
            c.R = (byte) (r1 * c1.R + r * c2.R);
            c.G = (byte) (r1 * c1.G + r * c2.G);
            c.B = (byte) (r1 * c1.B + r * c2.B);
            return Stringify(c);
        }
        public string Shade(string s, int light)
        {
            Color c = this[s];
            c.R = (byte)Math.Max(0, c.R - 255 + light);
            c.G = (byte)Math.Max(0, c.G - 255 + light);
            c.B = (byte)Math.Max(0, c.B - 255 + light);
            return Stringify(c);
        }
    }
}
