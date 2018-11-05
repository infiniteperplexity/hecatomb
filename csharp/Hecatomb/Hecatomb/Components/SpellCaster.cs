/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/7/2018
 * Time: 12:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hecatomb
{
	/// <summary>
	/// Description of SpellCaster.
	/// </summary>
	public class SpellCaster : Component, IChoiceMenu
	{
		List<string> Spells;
		public string MenuHeader
		{
			get
			{
				return "Choose a spell:";
			}
			set {}
		}
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
			Spells = new List<string>() {
                "RaiseZombieSpell",
                "TestGhoulSpell"
            };
		}
		
		public Spell GetSpell(Type t)
		{
			return (Spell) Activator.CreateInstance(t);
		}
		
		public Spell GetSpell(String s)
		{
			Type t = Type.GetType("Hecatomb."+s);
			return (Spell) Activator.CreateInstance(t);
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
