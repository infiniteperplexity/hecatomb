﻿/*
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
    using static HecatombAliases;
	/// <summary>
	/// Description of Spell.
	/// </summary>
	public class Spell : IMenuListable
	{
		public string MenuName;
		protected int cost;
		public SpellCaster Component;
		public Creature Caster;
        public bool ForDebugging;
        public string[] Researches;
        public string[] Structures;
		
		
		
		public Spell(): base()
		{
            Researches = new string[0];
            Structures = new string[0];
			cost = 10;
		}
		
		public virtual void Cast()
		{
            if (!Options.NoManaCost)
            {
                Component.Sanity -= GetCost();
            }
            Caster.GetComponent<Actor>().Spend();
            if (Caster==Player)
            {
                Commands.Act();
                ControlContext.Reset();
            }            
		}
		
		public virtual int GetCost()
		{
			return cost;
		}
		
		public virtual void ChooseFromMenu()
		{
		}
		
		
		public virtual ColoredText ListOnMenu()
		{
            if (GetCost()>Component.Sanity)
            {
                return "{gray}" + MenuName + " (" + GetCost() + ")";
            }
			return MenuName + " (" + GetCost() + ")"; ;
		}

        public string GetHighlightColor()
        {
            return "magenta";
        }

    }
	
	

    public class TestGhoulSpell : Spell, ISelectsTile
    {
        public TestGhoulSpell() : base()
        {
            cost = 0;
            MenuName = "Summon test ghoul.";
        }

        public override void ChooseFromMenu()
        {
            ControlContext.Set(new SelectTileControls(this));
        }

        public void SelectTile(Coord c)
        {
            ParticleEmitter emitter = new ParticleEmitter();
            emitter.Place(c.X, c.Y, c.Z);
            Creature zombie = Hecatomb.Entity.Spawn<Creature>("HungryGhoul");
            zombie.Place(c.X, c.Y, c.Z);
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Game.Controls.MenuMiddle = new List<ColoredText>() { "Spawn a test ghoul here." };
        }


    }
}


