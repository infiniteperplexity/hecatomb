using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    public class Harvestable : Component
    {
        public Dictionary<string, float> Yields;
        public int Labor;

        public Harvestable() : base()
        {
            Labor = 10;
        }

        public void Harvest()
        {
            int x = Entity.X;
            int y = Entity.Y;
            int z = Entity.Z;
            Dictionary<string, int> resources = new Dictionary<string, int>();
            foreach (string key in Yields.Keys)
            {
                if (Yields[key] < 1)
                {
                    if (Game.World.Random.NextDouble() < Yields[key])
                    {
                        resources[key] = 1;
                    }
                }
                else
                {
                    resources[key] = (int)Yields[key];
                }
            }
            if (resources.Count > 0)
            {
                var item = Hecatomb.Entity.Spawn<Item>();
                item.AddResources(resources);
                item.Owned = true;
                item.Place(x, y, z);
            }
            Entity.Despawn();
        }

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var yield = obj["Yields"];
            Yields = yield.ToObject<Dictionary<string, float>>();
        }
    }

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
