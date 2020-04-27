using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Corpse : Item
    {
        Species Species;
        public Corpse()
        {
            Species = Species.NoSpecies;
            StackSize = 1;
            Resource = Resource.Corpse;
        }
        public static Corpse SpawnNewCorpse(Species? species = null)
        {
            var corpse = Entity.Spawn<Corpse>();
            corpse.Species = (species is null) ? Species.Human : species;
            return corpse;
        }
    }
}
