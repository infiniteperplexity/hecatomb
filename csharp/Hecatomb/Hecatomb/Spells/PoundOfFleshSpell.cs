using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    class PoundOfFleshSpell : Spell
    {
        public PoundOfFleshSpell()
        {
            MenuName = "pound of flesh";
            cost = 5;
            Researches = new[] { "PoundOfFlesh" };
            Structures = new[] { "Sanctum" };
        }
    }
}
