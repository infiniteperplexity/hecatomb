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
			var minions = master.GetComponent<Minions>();
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
			Feature f = Game.World.Features[c.X, c.Y, c.Z];
			if (Game.World.Explored.Contains(c) && f!=null && f.TypeName=="Grave")
			{
				//ParticleEmitter emitter = new ParticleEmitter();
				//emitter.Place(c.X, c.Y, c.Z);
				f.Destroy();
				Creature zombie = Game.World.Entities.Spawn<Creature>("Zombie");
				zombie.Place(c.X, c.Y, c.Z);
				Game.World.Player.GetComponent<Minions>().Add(zombie);
			}
			else
			{
				// not sure what to do, if there's no message scroll
			}
		}
		
		public void TileHover(Coord c)
		{
			int x = c.X;
			int y = c.Y;
			int z = c.Z;
			Feature f = Game.World.Features[x, y, z];
			if (!Game.World.Explored.Contains(c))
			{
				Game.Controls.MenuMiddle = new List<string>() {"Unexplored tile."};
				Game.Controls.MiddleColors = ControlContext.InvalidColor;
			}
			else if (f!=null && f.TypeName=="Grave")
			{
				Game.Controls.MenuMiddle = new List<string>() {String.Format("Raise a zombie at {0} {1} {2}", x, y, z)};
				Game.Controls.MiddleColors = ControlContext.ValidColor;
			}
			else
			{
				Game.Controls.MenuMiddle = new List<string>() {"Select a tile with a tombstone or corpse."};
				Game.Controls.MiddleColors = ControlContext.InvalidColor;;
			}
		}
	}
	
}
