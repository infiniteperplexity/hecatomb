using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class SpellCaster : Component, IChoiceMenu
    {
        public List<Type> Spells;
        public int Sanity = 10;
        private int _maxSanity = 10;
        public int MaxSanity { get => _getMaxSanity(); }

        public SpellCaster()
        {
            Spells = new List<Type>();
        }

        private int _getMaxSanity()
        {
            return _maxSanity;
        }

        public void BuildMenu(MenuChoiceControls menu)
        {
            menu.Header = "Choose a spell:";
            List<IMenuListable> spells = new List<IMenuListable>();
            // only if we have the prerequisite structures / technologies...
            //var structures = Structure.ListAsStrings();
            //var researched = GetState<ResearchHandler>().Researched;
            var caster = Player.GetComponent<SpellCaster>();
            foreach (Type sp in Spells)
            {
                var spell = (Spell)Activator.CreateInstance(sp)!;
                spell.Caster = Player;
                spell.Component = caster;
                spells.Add(spell);
                //    var spell = GetSpell(sp);
                //    bool valid = true;
                //    if (spell.ForDebugging && !Options.ShowDebugSpells)
                //    {
                //        valid = false;
                //    }
                //    foreach (string s in spell.Researches)
                //    {
                //        if (!researched.Contains(s) && !Options.AllSpells)
                //        {
                //            valid = false;
                //        }
                //    }
                //    foreach (string s in spell.Structures)
                //    {
                //        if (!structures.Contains(s) && !Options.AllSpells)
                //        {
                //            valid = false;
                //        }
                //    }
                //    if (valid)
                //    {
                //        spells.Add(spell);
                //    }
            }
            menu.Choices = spells;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {
            //menu.MenuTop.Insert(1, Player.GetComponent<SpellCaster>().GetSanityText());
            //menu.MenuTop.Insert(1, " ");
        }
    }
}
