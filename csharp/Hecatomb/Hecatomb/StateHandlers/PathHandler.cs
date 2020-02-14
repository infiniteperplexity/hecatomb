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
        public Dictionary<int, Dictionary<int, (int, LinkedList<Coord>)>> PathHits;

        public PathHandler() : base()
        {
            PathMisses = new Dictionary<int, Dictionary<int, int>>();
            PathHits = new Dictionary<int, Dictionary<int, (int, LinkedList<Coord>)>>();
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public void ResetPaths()
        {
            PathMisses.Clear();
            PathHits.Clear();
        }

        public GameEvent OnTurnBegin(GameEvent g)
        {
            throw new Exception("Deeply buried exception test.");
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
            foreach (int eid1 in PathHits.Keys.ToList())
            {
                var dict = PathHits[eid1];
                foreach (int eid2 in dict.Keys.ToList())
                {
                    var tuple = dict[eid2];
                    dict[eid2] = (tuple.Item1 - 1, tuple.Item2);
                    if (dict[eid2].Item1 <= 0)
                    {
                        dict.Remove(eid2);
                        if (dict.Count == 0)
                        {
                            PathHits.Remove(eid1);
                        }
                    }
                    if (dict.Count == 0)
                    {

                    }
                }
            }
            return g;
        }
    }
}
