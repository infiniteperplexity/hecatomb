using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Hecatomb
{
    class RandomPaletteComponent : Component
    {
        public string RandomPaletteType;

        public string GetDisplayName()
        {
            if (Entity.Unbox().TypeName == "Flower")
            {
                foreach (var tuple in RandomPaletteHandler.FlowerNames)
                {
                    if (tuple.Item1 == RandomPaletteType)
                    {
                        return tuple.Item2;
                    }
                }
            }
            // shouldn't reach this
            return RandomPaletteType;
        }

        public string GetFG()
        {
            if (Entity.Unbox().TypeName == "Flower")
            {
                return OldGame.World.GetState<RandomPaletteHandler>().GetFlowerColor(RandomPaletteType);
            }
            // shouldn't reach this
            return "white";
        }
    }
}
