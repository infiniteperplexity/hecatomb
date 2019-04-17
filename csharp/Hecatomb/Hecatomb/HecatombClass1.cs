using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Hecatomb;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HecatombExtensions
{

    public static class ChoiceMenuExtensions
    {
        public static List<(Keys, IMenuListable)> ExtraChoices(this IChoiceMenu menu)
        {
            return new List<(Keys, IMenuListable)>();
        }
        public static List<IMenuListable> MenuChoices(this IChoiceMenu menu)
        {
            Debug.WriteLine("this thing");
            return new List<IMenuListable>();
        }
        public static void SetUp(this IChoiceMenu menu)
        {

        }
        public static void CleanUp(this IChoiceMenu menu)
        {

        }
    }
}