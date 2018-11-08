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
		protected int cost;
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
            Game.World.Events.Publish(new PlayerActionEvent() { ActionType = "ChooseSpell", Details = new Dictionary<string, object>() { { "Spell", this} } });
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
            base.ChooseFromMenu();
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
                Component.Sanity -= GetCost();
                ParticleEmitter emitter = new ParticleEmitter();
				emitter.Place(c.X, c.Y, c.Z);
//				f.Destroy();
				Creature zombie = Game.World.Entities.Spawn<Creature>("Zombie");
				zombie.Place(c.X, c.Y, c.Z-1);
				TaskEntity emerge = Game.World.Entities.Spawn<TaskEntity>("ZombieEmergeTask");
				emerge.GetComponent<Task>().AssignTo(zombie);
				emerge.Place(c.X, c.Y, c.Z);
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
	
	public class ZombieEmergeTask : Task
	{
		public override void Start()
		{
			Feature f = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
			if (f==null)
			{
				base.Start();
				f = Game.World.Features[Entity.X, Entity.Y, Entity.Z];
				f.Symbol = '\u2717';
				f.FG = "white";
			}
		}
		public override void Finish()
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			Feature f = Game.World.Features[x, y, z];
			f.Destroy();
			foreach (Coord c in Tiles.GetNeighbors8(x, y, z))
			{
			    int x1 = c.X;
			    int y1 = c.Y;
			    int z1 = c.Z;
			    f = Game.World.Features[x1, y1, z1];
			    if (Game.World.Features[x1, y1, z1]==null && !Game.World.Tiles[x1, y1, z1].Solid && !Game.World.Tiles[x1, y1, z1].Fallable)
			    {
			    	if (Game.World.Random.Next(2)==0)
			    	{
                        Item.PlaceResource(("Rock", 1), x1, y1, z1);
                        Item item = Game.World.Items[x1, y1, z1];
                        item.Owned = true;
			    	}
			    }
			}
			Game.World.Tiles[x, y, z] = Terrain.DownSlopeTile;
			Game.World.Tiles[x, y, z-1] = Terrain.UpSlopeTile;
            Game.World.Covers[x, y, z] = Cover.NoCover;
            Game.World.Covers[x, y, z - 1] = Cover.NoCover;
			base.Finish();
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
            Creature zombie = Game.World.Entities.Spawn<Creature>("HungryGhoul");
            zombie.Place(c.X, c.Y, c.Z);
        }

        public void TileHover(Coord c)
        {
            int x = c.X;
            int y = c.Y;
            int z = c.Z;
            Game.Controls.MenuMiddle = new List<string>() { "Spawn a test ghoul here." };
            Game.Controls.MiddleColors = ControlContext.ValidColor;
        }
    }
}


