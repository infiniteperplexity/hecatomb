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
                if (spell.ForDebugging && !Options.ShowDebugSpells)
                {
                    valid = false;
                }
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
            menu.MenuTop.Insert(1, Player.GetComponent<SpellCaster>().GetSanityText());
            menu.MenuTop.Insert(1, " ");
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            int max = GetCalculatedMaxSanity();
            int chance = 10;
            if (max > 20)
            {
                chance = 7;
            }
            var (x, y, z) = Entity;
            var f = Features[x, y, z];
            if (f != null && f.TryComponent<StructuralComponent>() != null)
            {
                if (f.GetComponent<StructuralComponent>().Structure.Unbox() is Sanctum)
                {
                    chance /= 2;
                }
            }
            if (Game.World.Random.Arbitrary(chance, OwnSeed())==0)
            //if (Game.World.Random.Next(chance)==0)
            {
                Sanity = Math.Min(max, Sanity + 1);
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
                "SiphonFleshSpell",
                "ShadowHopSpell",
                "DebugZombieSpell",
                "DebugBanditSpell",
                "DebugItemSpell",
                "SummonBanditsDebugSpell",
                "RemoveCreatureDebugSpell",
                "SevereDamageDebugSpell",
                "DebugHealSpell",
                "ParticleTestDebugSpell",
                "DebugFlowerSpell",
                "CrashDebugSpell"
            };
            AddListener<TurnBeginEvent>(OnTurnBegin);
		}

        public int GetCalculatedMaxSanity()
        {
            int calcMax = MaxSanity;
            if (Structure.ListAsStrings().Contains("Sanctum"))
            {
                calcMax += 5;
            }
            return calcMax;
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

        public ColoredText GetSanityText()
        {
            return $"Sanity: {Sanity}/{GetCalculatedMaxSanity()}";
        }
	}
}
