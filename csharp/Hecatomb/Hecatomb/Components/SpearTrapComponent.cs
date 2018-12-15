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
                Debug.WriteLine("stepping on trap");
                if ((pe.Entity as TypedEntity).GetComponent<Actor>().Team!=Team.PlayerTeam)
                {
                    Debug.WriteLine("trap should go off");
                }
            }

            return ge;
        }
    }
}
