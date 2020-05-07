using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;

    class NatureHandler : StateHandler
    {
        public int PastNatureAttacks;
        public List<int> Avengers;
        public int TreesKilled;
        public int TreesSince;

        public NatureHandler()
        {
            AddListener<DestroyEvent>(OnDestroy);
            Avengers = new List<int>();
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            var minTrees = 20;
            var sinceTrees = 8;
            var chance = 20;
            DestroyEvent de = (DestroyEvent)ge;
            if (de.Entity is ClubTree || de.Entity is SpadeTree && !HecatombOptions.NoDryads)
            {
                Feature f = (Feature)de.Entity;
                TreesKilled += 1;
                TreesSince += 1;
                if (f.Spawned && f.Placed && TreesKilled > minTrees && TreesSince >= sinceTrees && GameState.World!.Random.Next(chance) == 0)
                {
                    var (x, y, z) = f.GetPlacedCoordinate();
                    NatureAttack(x, y, z);
                }
            }
            return ge;
        }

        public void NatureAttack(int x, int y, int z)
        {
            TreesSince = 0;
            InterfaceState.Splash(new List<ColoredText> {
               "Your wanton deforestation has attracted the attention of nature's defenders!"
            },
            sleep: 2500,
            callback: InterfaceState.ResetControls,
            logText: "{red}You have angered nature's defenders!"); ;

            var trees = Features.Where((Feature f) => ((f is ClubTree || f is SpadeTree) && Tiles.Distance(x, y, z, (int)f.X!, (int)f.Y!, (int)f.Z!) < 25)).ToList();
            trees = trees.OrderBy((Feature f) => GameState.World!.Random.NextDouble()).ToList();
            for (int i = 0; i < PastNatureAttacks + 1; i++)
            {
                if (i > trees.Count - 1)
                {
                    break;
                }
                Feature f = trees[i];
                var (X, Y, Z) = f.GetPlacedCoordinate();
                if (Creatures.GetWithBoundsChecked(X, Y, Z) is null)
                {
                    var dryad = Entity.Spawn<Dryad>();
                    ParticleEmitter emitter = new ParticleEmitter();
                    emitter.Place(X, Y, Z);
                    dryad.PlaceInValidEmptyTile(X, Y, Z);
                    Activity.TargetPlayer.Act(dryad.GetComponent<Actor>(), dryad);
                }
            }
            PastNatureAttacks += 1;
        }
    }
}
