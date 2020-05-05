using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    /// <summary>
    /// Description of Spell.
    /// </summary>
    public class Spell : IMenuListable
    {
        public string MenuName;
        protected int _cost;
        public int Cost { get => (HecatombOptions.NoManaCost) ? 0 : getCost(); }
        public SpellCaster? Component;
        public Creature? Caster;
        public bool DebugginSpell;
        public Research[] RequiresResearch;
        public Type[] RequiresStructures;



        public Spell() : base()
        {
            MenuName = "";
            RequiresResearch = new Research[0];
            RequiresStructures = new Type[0];
            _cost = 10;
        }

        protected virtual int getCost()
        {
            return _cost;
        }
        public virtual void Cast()
        {
            Component!.Sanity -= Cost;
            //if (!Options.NoManaCost)
            //{
            //    Component.Sanity -= GetCost();
            //}
            //Caster.GetComponent<Actor>().Spend();
            //if (Caster == Player)
            //{
            //    Commands.Act();
            //    ControlContext.Reset();
            //}
        }


        public virtual void ChooseFromMenu()
        {
        }


        public virtual ColoredText ListOnMenu()
        {
            if (Cost > Component!.Sanity)
            {
                return "{gray}" + MenuName + " (" + Cost+ ")";
            }
            return MenuName + " (" + Cost + ")"; ;
        }

        public string GetHighlightColor()
        {
            return "magenta";
        }

    }
}


