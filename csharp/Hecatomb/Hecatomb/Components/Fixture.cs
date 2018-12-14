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
        public Dictionary<string, int> Ingredients;
        public string[] Structures;
        public string[] Research;
        public int Labor;

        public Fixture() : base()
        {
            Structures = new string[0];
            Research = new string[0];
            Labor = 10;
        }



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
                Structures = structures.ToObject<string[]>();
            }
            var research = obj["Research"];
            if (research != null)
            {
                Research = research.ToObject<string[]>();
            }
        }
    }
}
