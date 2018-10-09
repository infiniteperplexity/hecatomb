/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/7/2018
 * Time: 12:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of Spell.
	/// </summary>
	public class Spell : IMenuListable
	{

		public string MenuName;
		private int cost;
		public SpellCaster Component;
		public Creature Caster;
		
		
		
		public Spell(): base()
		{
			cost = 10;
		}
		
		public virtual void Cast()
		{
			
		}
		
		public virtual int GetCost()
		{
			return cost;
		}
		
		public virtual void ChooseFromMenu()
		{
//			Game.Controls.Set(new SelectTileControls(this));
		}
		
		
		public virtual string ListOnMenu()
		{
			return MenuName;
		}
		
	}
	
	public class RaiseZombieSpell : Spell, ISelectsTile
	{
		public RaiseZombieSpell() : base()
		{
			MenuName = "Raise zombie.";
		}
		public override void Cast()
		{
			
		}
		
		public override int GetCost()
		{
			Player master = (Player) Caster;
			List<Creature> minions = master.GetMinions();
			if (minions.Count==0)
			{
				return 10;
			}
			else if (minions.Count==1)
			{
				return 15;
			}
			else if (minions.Count==2)
			{
				return 20;
			}
			else
			{
				return 25;
			}
		}
		
		public override void ChooseFromMenu()
		{
			Game.Controls.Set(new SelectTileControls(this));
		}
		
		public void SelectTile(Coord c)
		{
			if (Game.World.Creatures[c.x, c.y, c.z]==null) 
			{
				Creature zombie = Game.World.Entities.Spawn<Creature>("Zombie");
				zombie.Place(c.x, c.y, c.z);
				Game.World.Player.AddMinion(zombie);
			}
		}
		
		public void TileHover(Coord c)
		{
			Debug.Print("Raise a zombie at {0} {1} {2}", c.x, c.y, c.z);
		}
	}
	
}
