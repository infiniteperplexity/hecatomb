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
	/// <summary>
	/// Description of SpellCaster.
	/// </summary>
	public class SpellCaster : Component, IChoiceMenu
	{
		List<string> Spells;
        public int MaxSanity;
        public int Sanity;
		[JsonIgnore] public string MenuHeader
		{
			get
			{
				return "Choose a spell:";
			}
			set {}
		}

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Game.World.Random.Next(10)==0)
            {
                Sanity = Math.Min(MaxSanity, Sanity + 1);
            }
            return ge;
        }
        [JsonIgnore]
        public List<IMenuListable> MenuChoices
		{
			get
			{
				List<IMenuListable> spells = new List<IMenuListable>();
				foreach (string s in Spells)
				{
					spells.Add(GetSpell(s));
				}
				return spells;
			}
			set {}
		}  
		
		public SpellCaster() : base()
		{
            MaxSanity = 20;
            Sanity = 20;
			Spells = new List<string>() {
                "RaiseZombieSpell",
                "TestGhoulSpell"
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
		
//		public List<Spell> GetSpells()
//		{
//			List<Spell> spells = new List<Spell>();
//			foreach (string s in Spells)
//			{
//				spells.Add(GetSpell(s));
//			}
//			return spells;
//		}
		public T GetSpell<T>() where T : Spell, new()
		{
			T s = new T();
			s.Component = this;
			s.Caster = (Creature) this.Entity;
			return s;
		}
	}
}
