﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Harvestable : Component
    {
        public Dictionary<Resource, float> Yields = new Dictionary<Resource, float>();
        public int Labor = 10;

        public void Harvest()
        {
            var (x, y, z) = Entity.UpdateNullity()!.UnboxIfNotNull()!;
            Dictionary<Resource, int> resources = new Dictionary<Resource, int>();
            foreach (var key in Yields.Keys)
            {
                int n = (int)Yields[key];
                float remainder = Yields[key] - n;
                if (GameState.World!.Random.NextDouble()<remainder)
                {
                    n += 1;
                }
                if (key == Resource.Gold)
                {
                    Publish(new AchievementEvent() { Action = "FoundGold" });
                }
                if (key == Resource.Corpse)
                {
                    var item = Corpse.SpawnNewCorpse();
                    item.DropOnValidTile((int)x!, (int)y!, (int)z!);
                }
                else
                {
                    var item = Item.SpawnNewResource(key, n);
                    item.DropOnValidTile((int)x!, (int)y!, (int)z!);
                }
            }
            Entity.UnboxIfNotNull()!.Destroy();
        }
    }
}
