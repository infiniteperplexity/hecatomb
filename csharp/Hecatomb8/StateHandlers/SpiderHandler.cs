using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hecatomb
{
    class SpiderHandler : StateHandler
    {
        public SpiderHandler()
        {
            AddListener<ActEvent>(OnAct);
        }

        public GameEvent OnAct(GameEvent ge)
        {
            // this is all preliminary
            ActEvent ae = (ActEvent)ge;
            Actor actor = ae.Actor;
            TypedEntity te = actor.Entity.Unbox();
            if (te.TypeName == "Spider" && !actor.Acted)
            {
                if (OldGame.World.Random.Arbitrary(250, actor.OwnSeed()) == 0)
                {
                    var (x, y, z) = te;
                    if (OldGame.World.Features[x, y, z] == null && OldGame.World.Terrains[x, y, z] == Terrain.FloorTile)
                    {
                        Entity.Spawn<Feature>("SpiderWeb").Place(x, y, z);
                        actor.Spend(16);
                    }

                }
            }
            return ge;
        }
    }
}
