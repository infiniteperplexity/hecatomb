using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Corpse : Item
    {
        public Species Species;
        [JsonIgnore] public int MaxDecay;
        public int Decay;

        public Corpse()
        {
            Species = Species.NoSpecies;
            StackSize = 1;
            Resource = Resource.Corpse;
            MaxDecay = 500;
            Decay = MaxDecay;
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            Decay -= 1;
            if (Decay == 0)
            {
                Destroy();
            }
            return ge;
        }

        protected override string getFG()
        {
            double frac = (double)Decay / (double)MaxDecay;
            if (frac < 0.25)
            {
                return "purple";
            }
            else if (frac < 0.5)
            {
                return "olive";
            }
            return Resource!.FG;
        }

        public static Corpse SpawnNewCorpse(Species? species = null)
        {
            var corpse = Entity.Spawn<Corpse>();
            corpse.Species = (species is null) ? Species.Human : species;
            return corpse;
        }
        protected override string? getName()
        {
            var name = "corpse";
            if (Disowned)
            {
                name = name + " (unclaimed)";
            }
            if (Species != Species.Human)
            {
                name = Species.Name + " " + name;
            }
            double frac = (double)Decay / (double)MaxDecay;
            if (frac < 0.25)
            {
                name = "severely rotted " + name;
            }
            else if (frac < 0.5)
            {
                name = "rotted " + name;
            }
            else if (frac < 0.75)
            {
                name = "slightly rotted " + name;
            }
            return name;
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
