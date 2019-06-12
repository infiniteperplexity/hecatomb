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

    class NatureTracker : EncounterTracker
    {
        public int PastNatureAttacks;
        public List<EntityField<Creature>> Avengers;
        public int TreesKilled;

        public NatureTracker()
        {
            AddListener<DestroyEvent>(OnDestroy);
            AddListener<ActEvent>(OnAct);
            Avengers = new List<EntityField<Creature>>();
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent de = (DestroyEvent)ge;
            if (de.Entity is Feature)
            {
                Feature f = (Feature)de.Entity;
                if (f.TypeName == "ClubTree" || f.TypeName == "SpadeTree")
                {
                    TreesKilled += 1;
                    if (TreesKilled > 25 && Game.World.Random.Next(25) == 0)
                    {
                        NatureAttack();
                    }
                }
            }
            return ge;
        }

        public GameEvent OnAct(GameEvent ge)
        {
            return ge;
        }

        public void NatureAttack()
        {
            Game.SplashPanel.Splash(new List<ColoredText> {
               "Your wanton deforestation has attracted the attention of nature's defenders!"
            });
            
            PastNatureAttacks += 1;
        }
    }
}
