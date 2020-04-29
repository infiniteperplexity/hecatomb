using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb8
{
    using static HecatombAliases;

    public class Fixture : Component
    {
        public Dictionary<string, int> Ingredients = new Dictionary<string, int>();
        public string[] StructurePrereqs = new string[0];
        public string[] ResearchPrereqs = new string[0];
        public int Labor = 10;
    }
}
