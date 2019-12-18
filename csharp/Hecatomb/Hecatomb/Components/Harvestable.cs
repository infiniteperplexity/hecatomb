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
                int n = 0;
                if (Yields[key] < 1)
                {
                    if (Game.World.Random.NextDouble() < Yields[key])
                    {
                        n = 1;
                    }
                }
                else
                {
                    n = (int) Yields[key];
                }
                if (n > 0)
                {
                    if (key == "Corpse")
                    {
                        Item.SpawnCorpse().Place(Entity.X, Entity.Y, Entity.Z);
                    }
                    else
                    {
                        Item.PlaceNewResource(key, n, Entity.X, Entity.Y, Entity.Z);
                    }
                }
            }
            Entity.Unbox().Destroy();
        }

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var yield = obj["Yields"];
            Yields = yield.ToObject<Dictionary<string, float>>();
        }
    }
}
