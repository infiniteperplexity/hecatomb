using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class IncompleteFixture : Feature
    {
        public Type? Makes;
        public ListenerHandledEntityHandle<Structure>? Structure;
        public string IncompleteFG = "brown";
        public char IncompleteSymbol = '\u25AB';

        public IncompleteFixture()
        {
            _name = "incomplete fixture";
            _fg = "brown";
            _symbol = '\u25AB';
        }

        protected override string? getName()
        {
            // let's just be defensive
            if (Makes is null)
            {
                return base.getName();
            }
            return "incomplete " + (Entity.Mock(Makes!) as TileEntity)!.Name;
        }
        protected override string? getFG()
        {
            return IncompleteFG;
        }
        protected override char getSymbol()
        {
            return IncompleteSymbol;
        }
    }
}
