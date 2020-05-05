using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class SpellCaster : Component, IDisplayInfo
    {
        public List<Type> Spells;
        public int Sanity = 20;
        private int _maxSanity = 20;
        public int MaxSanity { get => _getMaxSanity(); }

        public SpellCaster()
        {
            Spells = new List<Type>();
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        private int _getMaxSanity()
        {
            int calculated = _maxSanity;
            if (Structure.ListStructureTypes().Contains(typeof(Sanctum)))
            {
                calculated += 5;
            }
            return calculated;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return ge;
            }
            int chance = 9;
            if (MaxSanity > 20)
            {
                chance = 5;
            }
            var (x, y, z) = Entity.UnboxBriefly()!.GetValidCoordinate();
            var f = Features.GetWithBoundsChecked(x, y, z);
            if (f is StructuralFeature)
            {
                if ((f as StructuralFeature)!.Structure?.UnboxBriefly() is Sanctum)
                {
                    chance /= 2;
                }
            }
            int r = GameState.World!.Random.Next(chance);
            if (r==0)
            {
                Sanity = Math.Min(MaxSanity, Sanity + 1);
            }

            return ge;
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            menu.Header = "Choose a spell:";
            List<IMenuListable> spells = new List<IMenuListable>();
            // only if we have the prerequisite structures / technologies...
            var structures = Structure.ListStructureTypes();
            var researched = GetState<ResearchHandler>().Researched;
            var caster = Player.GetComponent<SpellCaster>();
            foreach (Type sp in Spells)
            {
                var spell = (Spell)Activator.CreateInstance(sp)!;
                bool valid = true;
                if (spell is DebugSpell && !HecatombOptions.ShowDebugSpells)
                {
                    valid = false;
                }
                foreach (Research r in spell.RequiresResearch)
                {
                    if (!researched.Contains(r) && !HecatombOptions.ShowAllSpells)
                    {
                        valid = false;
                    }
                }
                foreach (Type t in spell.RequiresStructures)
                {
                    if (!structures.Contains(t) && !HecatombOptions.ShowAllSpells)
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    spell.Caster = Player;
                    spell.Component = caster;
                    spells.Add(spell);
                }
            }
            menu.Choices = spells;
        }

        public T? GetSpell<T>() where T : Spell
        {
            foreach (Type t in Spells)
            {
                if (t == typeof(T))
                {
                    var spell = (T)Activator.CreateInstance(t)!;
                    spell.Caster = Player;
                    spell.Component = Player.GetComponent<SpellCaster>();
                    return spell;
                }
            }
            return null;
        }

        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.InfoTop.Insert(1, Player.GetComponent<SpellCaster>().GetSanityText());
            menu.InfoTop.Insert(1, " ");
        }

        public ColoredText GetSanityText()
        {
            return $"Sanity: {Sanity}/{MaxSanity}";
        }
    }
}
