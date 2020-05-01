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

        protected override string? getName()
        {
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
