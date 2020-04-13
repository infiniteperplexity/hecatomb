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
    public class SpearTrapComponent : Component
    {
        public SpearTrapComponent() : base()
        {
            AddListener<PlaceEvent>(OnStep);
        }
        public GameEvent OnStep(GameEvent ge)
        {
            PlaceEvent pe = (PlaceEvent)ge;
            if (pe.Entity is Creature && pe.X==Entity.X && pe.Y==Entity.Y && pe.Z==Entity.Z)
            {
                if ((pe.Entity as TypedEntity).GetComponent<Actor>().Team != Teams.Friendly)
                {
                    Entity.GetComponent<Attacker>().Attack((TypedEntity) pe.Entity);
                    if (OldGame.World.Random.Arbitrary(2, OwnSeed()) == 0)
                    //if (Game.World.Random.Next(2)==0)
                    {
                        Item.PlaceNewResource("Flint", 1, Entity.X, Entity.Y, Entity.Z);
                    }
                    else if (OldGame.World.Random.Arbitrary(2, OwnSeed()+1) == 0)
                    //else if (Game.World.Random.Next(2) == 0)
                    {
                        Item.PlaceNewResource("Wood", 1, Entity.X, Entity.Y, Entity.Z);
                    }
                    Entity.Unbox().Destroy();
                    
                }
            }

            return ge;
        }
    }
}
