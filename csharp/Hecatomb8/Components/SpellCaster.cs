﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class SpellCaster : Component, IChoiceMenu
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
            return _maxSanity;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Entity?.UnboxBriefly() is null)
            {
                return ge;
            }
            int chance = 5;
            if (MaxSanity > 20)
            {
                chance = 7;
            }
            //var (x, y, z) = Entity.UnboxBriefly(); ;
            //var f = Features[x, y, z];
            //if (f != null && f.TryComponent<StructuralComponent>() != null)
            //{
            //    if (f.GetComponent<StructuralComponent>().Structure.Unbox() is Sanctum)
            //    {
            //        chance /= 2;
            //    }
            //}
            int r = GameState.World!.Random.Next(chance);
            if (r==0)
            {
                Sanity = Math.Min(MaxSanity, Sanity + 1);
                Debug.WriteLine("actual sanity is " + Sanity);
            }

            return ge;
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

        public void FinishMenu(MenuChoiceControls menu)
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
