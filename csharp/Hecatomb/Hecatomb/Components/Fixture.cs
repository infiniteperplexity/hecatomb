using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    using static HecatombAliases;

    public class Fixture : Component
    {
        public Dictionary<string, int> Ingredients = new Dictionary<string, int>();
        public string[] StructurePrereqs = new string[0];
        public string[] ResearchPrereqs = new string[0];
        public int Labor = 10;

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var ingredients = obj["Ingredients"];
            if (ingredients != null)
            {
                Ingredients = ingredients.ToObject<Dictionary<string, int>>();
            }
            var structures = obj["Structures"];
            if (structures != null)
            {
                StructurePrereqs = structures.ToObject<string[]>();
            }
            var research = obj["Research"];
            if (research != null)
            {
                ResearchPrereqs = research.ToObject<string[]>();
            }
        }
    }
}
