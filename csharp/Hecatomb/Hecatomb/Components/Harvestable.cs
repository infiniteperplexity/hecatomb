using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    public class Harvestable : Component
    {
        public Dictionary<string, float> Yields = new Dictionary<string, float>();
        public int Labor = 10;

        public void Harvest()
        {
            var (x, y, z) = Entity;
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
}
