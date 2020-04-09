using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Hecatomb
{
    public interface ICallStrings
    {
        void CallString(string s);
    }

    public static class ICallStringsExtensions
    {
        public static void CallString(this ICallStrings ics, string s)
        {
            ics.GetType().GetMethod(s).Invoke(ics, null);
        }
    }
}
