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
			
		}
		
		public virtual int GetCost()
		{
			return cost;
		}
		
		public virtual void ChooseFromMenu()
		{
           
//			Game.Controls.Set(new SelectTileControls(this));
		}
		
		
		public virtual ColoredText ListOnMenu()
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
            var minions = GetState<TaskHandler>().Minions;
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
            base.ChooseFromMenu();
            Game.World.Events.Publish(new TutorialEvent() { Action = "ChooseRaiseZombie"});
            if (GetCost() > Component.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                Game.Controls.Set(new SelectTileControls(this));
            }
		}
		
		public void SelectTile(Coord c)
		{
			Feature f = Game.World.Features[c.X, c.Y, c.Z];
			if (Game.World.Explored.Contains(c) && f!=null && f.TypeName=="Grave")
			{
                Game.World.Events.Publish(new TutorialEvent() { Action = "CastRaiseZombie" });
                Game.World.Events.Publish(new AchievementEvent() { Action = "CastRaiseZombie" });
                Component.Sanity -= GetCost();
                ParticleEmitter emitter = new ParticleEmitter();
				emitter.Place(c.X, c.Y, c.Z);
//				f.Destroy();
				Creature zombie = Entity.Spawn<Creature>("Zombie");
                zombie.GetComponent<Actor>().Team = Team.PlayerTeam;
				zombie.Place(c.X, c.Y, c.Z-1);
				Task emerge = Entity.Spawn<ZombieEmergeTask>();
				emerge.AssignTo(zombie);
				emerge.Place(c.X, c.Y, c.Z);
                GetState<TaskHandler>().Minions.Add(zombie);   
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
				Game.Controls.MenuMiddle = new List<ColoredText>() {"{orange}Unexplored tile."};
			}
			else if (f!=null && f.TypeName=="Grave")
			{
				Game.Controls.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Raise a zombie at {0} {1} {2}", x, y, z)};
			}
			else
			{
				Game.Controls.MenuMiddle = new List<ColoredText>() {"{orange}Select a tile with a tombstone or corpse."};
			}
		}
	}
	
	public class ZombieEmergeTask : Task
	{
		public override void Start()
		{
            Game.World.Events.Publish(new SensoryEvent() { X = X, Y = Y, Z = Z, Sight = "You hear an ominous stirring from under the ground..." });
			Feature f = Game.World.Features[X, Y, Z];
			if (f==null)
			{
				base.Start();
				f = Game.World.Features[X, Y, Z];
				f.Symbol = '\u2717';
				f.FG = "white";
			}
		}
		public override void Finish()
		{
            
            Game.World.Events.Publish(new TutorialEvent() { Action = "ZombieEmerges" });
            Game.World.Events.Publish(new SensoryEvent() { Sight = "A zombie bursts forth from the ground!", X = X, Y = Y, Z = Z });
            Feature f = Game.World.Features[X, Y, Z];
			f.Destroy();
			foreach (Coord c in Tiles.GetNeighbors8(X, Y, Z))
			{
			    int x1 = c.X;
			    int y1 = c.Y;
			    int z1 = c.Z;
			    f = Game.World.Features[x1, y1, z1];
			    if (Game.World.Features[x1, y1, z1]==null && !Game.World.Terrains[x1, y1, z1].Solid && !Game.World.Terrains[x1, y1, z1].Fallable)
			    {
			    	if (Game.World.Random.Next(2)==0)
			    	{
                        Item.PlaceResource(("Rock", 1), x1, y1, z1);
                        Item item = Game.World.Items[x1, y1, z1];
                        item.Owned = true;
			    	}
			    }
			}
			Game.World.Terrains[X, Y, Z] = Terrain.DownSlopeTile;
			Game.World.Terrains[X, Y, Z - 1] = Terrain.UpSlopeTile;
            Game.World.Covers[X, Y, Z] = Cover.NoCover;
            Game.World.Covers[X, Y, Z - 1] = Cover.NoCover;
			base.Finish();
            Game.World.ValidateOutdoors();
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
            Game.Controls.Set(new SelectTileControls(this));
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


