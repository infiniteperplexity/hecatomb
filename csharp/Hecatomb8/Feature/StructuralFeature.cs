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

        public StructuralFeature()
        {
            _name = "structure";
            _fg = "FLOORFG";
            _bg = "FLOORBG";
            _symbol = '.';
        }

        // this approach to overriding might not scale well with increasing complexity
        protected override string? getName()
        {
            string name = base.getName()!;
            if (name != _name || StructuralName is null)
            {
                return name;
            }
            else
            {
                return StructuralName;
            }
        }
        protected override string? getFG()
        {
            string fg = base.getFG()!;
            if (fg != _fg || StructuralBG is null)
            {
                return fg;
            }
            else
            {
                return StructuralFG;
            }
        }
        protected override char getSymbol()
        {
            char sym = base.getSymbol()!;
            if (sym != _symbol || StructuralSymbol is null)
            {
                return sym;
            }
            else
            {
                return (char)StructuralSymbol!;
            }
        }
        protected override string? getBG()
        {
            string bg = base.getBG()!;
            if (bg != _bg || StructuralBG is null)
            {
                return bg;  
            }
            else
            {
                return StructuralBG;
            }
        }

        // it's kind of hard for me to imagine needing this...I suppose in theory you'd nullify the Structure?  I dunno...
        public override GameEvent OnDespawn(GameEvent ge)
        {
            return base.OnDespawn(ge);
        }
    }
}
