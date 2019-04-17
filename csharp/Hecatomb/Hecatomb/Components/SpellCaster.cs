/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/7/2018
 * Time: 12:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Hecatomb
{
    using static HecatombAliases;

	public class SpellCaster : Component, IChoiceMenu
	{
		List<string> Spells;
        public int MaxSanity;
        public int Sanity;
        public void BuildMenu(MenuChoiceControls menu)
        {
            menu.Header = "Choose a spell:";
            List<IMenuListable> spells = new List<IMenuListable>();
            // only if we have the prerequisite structures / technologies...
            var structures = Structure.ListAsStrings();
            var researched = GetState<ResearchHandler>().Researched;
            foreach (string sp in Spells)
            {
                var spell = GetSpell(sp);
                bool valid = true;
                foreach (string s in spell.Researches)
                {
                    if (!researched.Contains(s) && !Options.AllSpells)
                    {
                        valid = false;
                    }
                }
                foreach (string s in spell.Structures)
                {
                    if (!structures.Contains(s) && !Options.AllSpells)
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    spells.Add(spell);
                }
            }
            menu.Choices = spells;
        }
        public void FinishMenu(MenuChoiceControls menu)
        {

        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Game.World.Random.Next(10)==0)
            {
                Sanity = Math.Min(MaxSanity, Sanity + 1);
            }
            return ge;
        }
        
		public SpellCaster() : base()
		{
            MaxSanity = 20;
            Sanity = 20;
			Spells = new List<string>() {
                "RaiseZombieSpell",
                "CondenseEctoplasmSpell",
                "PoundOfFleshSpell",
                "LongShadowSpell",
                "SummonBanditsDebugSpell",
                "RemoveCreatureDebugSpell"
            };
            AddListener<TurnBeginEvent>(OnTurnBegin);
		}
		
		public Spell GetSpell(Type t)
		{
            Spell spell = (Spell)Activator.CreateInstance(t);
            spell.Component = this;
            spell.Caster = (Creature) Entity;
            return spell;
		}
		
		public Spell GetSpell(String s)
		{
			Type t = Type.GetType("Hecatomb."+s);
            Spell spell = (Spell) Activator.CreateInstance(t);
            spell.Component = this;
            spell.Caster = (Creature) Entity;
            return spell;
        }

		public T GetSpell<T>() where T : Spell, new()
		{
			T s = new T();
			s.Component = this;
			s.Caster = (Creature) this.Entity;
			return s;
		}
	}
}
