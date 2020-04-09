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
    class GrowthComponent : Component
    {
        public string Makes;
        public int Turns;

        public GrowthComponent() : base()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }
        public GameEvent OnTurnBegin(GameEvent ge)
        {
            Turns -= 1;
            if (Turns<=0)
            {
                Feature f = Hecatomb.Entity.Spawn<Feature>(Makes);
                var (x, y, z) = Entity;
                Entity.Despawn();
                f.Place(x, y, z);
            }
            return ge;
        }

        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            var makes = obj["Makes"];
            Makes = makes.ToObject<string>();
            var turns = obj["Turns"];
            Turns = turns.ToObject<int>();
        }
    }
}
