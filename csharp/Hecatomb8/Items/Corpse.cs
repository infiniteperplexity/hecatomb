using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Corpse : Item
    {
        public Species Species;
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
        protected override string? getName()
        {
            var unowned = (Disowned) ? " (unclaimed)" : "";
            if (Species == Species.Human)
            {
                return $"corpse {unowned}";
            }
            return $"{Species.Name} corpse{unowned}";
        }
        public override string Describe(
            bool? article = null,
            bool definite = false,
            bool capitalized = false
        )
        {
            bool Article = article ?? true;
            return base.Describe(article: Article, definite: definite, capitalized: capitalized);
        }
    }
}
