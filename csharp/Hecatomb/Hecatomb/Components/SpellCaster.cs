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

namespace Hecatomb
{
	/// <summary>
	/// Description of SpellCaster.
	/// </summary>
	public class SpellCaster : Component
	{
		List<Spell> Spells;
		public SpellCaster() : base()
		{
			Spells = new List<Spell>();
		}
	}
}
