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

    public class PathHandler : StateHandler
    {
        // cacheing misses is much more important than cacheing successes
        public Dictionary<int, Dictionary<int, int>> PathMisses;

        public PathHandler() : base()
        {
            PathMisses = new Dictionary<int, Dictionary<int, int>>();
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public void ResetPaths()
        {
            PathMisses.Clear();
        }

        public GameEvent OnTurnBegin(GameEvent g)
        {
            foreach (int eid1 in PathMisses.Keys.ToList())
            {
                var dict = PathMisses[eid1];
                foreach (int eid2 in dict.Keys.ToList())
                {
                    dict[eid2] -= 1;
                    if (dict[eid2] <= 0)
                    {
                        dict.Remove(eid2);
                        if (dict.Count == 0)
                        {
                            PathMisses.Remove(eid1);
                        }
                    }
                }
            }
            return g;
        }
    }
}
