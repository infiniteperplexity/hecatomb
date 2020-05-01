using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class StructuralFeature : Feature
    {
        public ListenerHandledEntityHandle<Structure>? Structure;
        public string? StructuralFG;
        public string? StructuralName;
        public string? StructuralBG;
        public char? StructuralSymbol;

        protected override string? getName()
        {
            return StructuralName ?? "structural feature";
        }
        protected override string? getFG()
        {
            return StructuralFG ?? "WALLFG";
        }
        protected override char getSymbol()
        {
            return StructuralSymbol ?? '.';
        }

        // it's kind of hard for me to imagine needing this...I suppose in theory you'd nullify the Structure?  I dunno...
        public override GameEvent OnDespawn(GameEvent ge)
        {
            return base.OnDespawn(ge);
        }
    }
}
