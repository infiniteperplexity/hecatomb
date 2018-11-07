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
                var item = Game.World.Entities.Spawn<Item>();
                item.AddResources(resources);
                item.Place(x, y, z);
            }
            Entity.Despawn();
        }

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var yield = obj["Yields"];
            Yields = yield.ToObject<Dictionary<string, float>>();
            Debug.WriteLine(Yields.Count);
        }
    }

    public class Fixture : Component
    {
        public Dictionary<string, int> Ingredients;
        public int Labor;

        public Fixture() : base()
        {
            Labor = 10;
        }

        

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var ingredients = obj["Ingredients"];
            Ingredients = ingredients.ToObject<Dictionary<string, int>>();
            Debug.WriteLine(Ingredients.Count);
        }
    }
}
