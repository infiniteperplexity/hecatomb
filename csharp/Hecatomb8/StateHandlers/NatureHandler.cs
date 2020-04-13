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
        public int TreesSince;

        public NatureTracker()
        {
            AddListener<DestroyEvent>(OnDestroy);
            //AddListener<ActEvent>(OnAct);
            Avengers = new List<EntityField<Creature>>();
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent de = (DestroyEvent)ge;
            if (de.Entity is Feature && OldGame.Options.NoNatureAttacks == false)
            {
                
                Feature f = (Feature)de.Entity;
                if (f.TypeName == "ClubTree" || f.TypeName == "SpadeTree")
                {
                    TreesKilled += 1;
                    TreesSince += 1;
                    if (TreesKilled > 20 && TreesSince >= 8 && OldGame.World.Random.Arbitrary(0, OwnSeed()) == 0)
                    //if (TreesKilled > 20 && TreesSince >= 8 && Game.World.Random.Next(20) == 0)
                    {
                        NatureAttack(f.X, f.Y, f.Z);
                    }
                }
            }
            return ge;
        }

        //public GameEvent OnAct(GameEvent ge)
        //{
        //    return ge;
        //}

        public void NatureAttack(int x, int y, int z)
        {
            TreesSince = 0;
            OldGame.SplashPanel.Splash(new List<ColoredText> {
               "Your wanton deforestation has attracted the attention of nature's defenders!"
            },
            logText: "{red}You have angered nature's defenders!");

            var trees = Features.Where((Feature f) => ((f.TypeName == "ClubTree" || f.TypeName == "SpadeTree") && Tiles.QuickDistance(x, y, z, f.X, f.Y, f.Z)<25)).ToList();
            trees = trees.OrderBy((Feature f) => OldGame.World.Random.Arbitrary(f.OwnSeed())).ToList();
            for (int i = 0; i<PastNatureAttacks+2; i++)
            {
                if (i>trees.Count-1)
                {
                    break;
                }
                Feature f = trees[i];

                if (Creatures[f.X, f.Y, f.Z] == null)
                {
                    var dryad = Entity.Spawn<Creature>("RagingDryad");
                    ParticleEmitter emitter = new ParticleEmitter();
                    emitter.Place(f.X, f.Y, f.Z);
                    dryad.Place(f.X, f.Y, f.Z);
                }
            }
            PastNatureAttacks += 1;
        }
    }
}
