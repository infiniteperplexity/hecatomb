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

    public class DebuggingListener : StateHandler
    {
        public HashSet<Coord> OldFloods;
        public DebuggingListener() : base()
        {
            //AddListener<DigEvent>(OnDig);
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public GameEvent OnDig(GameEvent ge)
        {
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            return ge;
        }
    }

}
