using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    class LongCreatureComponent : Component
    {
        public List<TypedEntityField<Creature>> Segments;
        public LongCreatureComponent()
        {
            Segments = new List<TypedEntityField<Creature>>();
            AddListener<DestroyEvent>(OnDestroy);
            AddListener<DespawnEvent>(OnDespawn);
            AddListener<PlaceEvent>(OnStep);
        }

        public void AddSegment()
        {

        }

        public void AddSegment(int x, int y, int z)
        {

        }
        public GameEvent OnDestroy(GameEvent ge)
        {
            return ge;
        }

        public GameEvent OnDespawn(GameEvent ge)
        {
            return ge;
        }

        public GameEvent OnStep(GameEvent ge)
        {
            return ge;
        }
    }
}
