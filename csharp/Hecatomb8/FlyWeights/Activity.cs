using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public delegate void Act(Actor a, Creature cr);

    public partial class Activity : FlyWeight<Activity>
    {
        [JsonIgnore]public Act Act;

        public static void NoAction(Actor a, Creature cr) { }

        public Activity(
           string type = "Activity",
           Act? act = null
           //Action<Actor, Creature>? act = null
        ) : base(type)
        {
            Act = act ?? NoAction;;
        }

        public static readonly Activity Example = new Activity(
            type: "Example",
            act: NoAction
        );
    }
}
