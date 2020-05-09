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
        public JsonArrayDictionary<Resource, int> Ingredients = new JsonArrayDictionary<Resource, int>();
        //public JsonArrayDictionary<Resource, int> RepairIngredients = new JsonArrayDictionary<Resource, int>();
        public Type[] RequiresStructures = new Type[0];
        public Research[] RequiresResearch = new Research[0];
        public int Labor = 10;
    }
}
