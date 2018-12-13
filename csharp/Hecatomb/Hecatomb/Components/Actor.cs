/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 1:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of Actor.
	/// </summary>
	/// 
	
	public class Actor : Component
	{
        public EntityField<PositionedEntity> Target;
		public int ActionPoints;
		public int CurrentPoints;
		private string TeamName;
        public bool Acted;
        public List<Action> Goals;
        public Action Alert;
        public Action Fallback;
        // so I guess this doesn't get reconstituted correctly when restoring a game
		[JsonIgnore] public Team Team
		{
			get
			{
				return (TeamName==null) ? null : Team.Types[TeamName];
			}
			set
			{
				if (value==null)
				{
					if (TeamName!=null)
					{
                        Team.Types[TeamName].RemoveMember((Creature) Entity);
					}
					TeamName = null;
				}
				else
				{
					value.AddMember((Creature) Entity);
					TeamName = value.TypeName;
				}
			}
		}
		public Actor() : base()
		{
			ActionPoints = 16;
            CurrentPoints = (Game.World.Turns.Turn==0) ? ActionPoints : 0;
            Alert = CheckForHostile;
            Goals = new List<Action>();
            Fallback = WalkRandom;
        }
		
		public void Regain()
		{
			while (CurrentPoints<=0)
			{
				CurrentPoints+=16;
			}
            Acted = false;
		}
		public void Spend(int i)
		{
			CurrentPoints-=i;
            Acted = true;
		}
		public void Spend()
		{
			Spend(16);
		}
		public void Act()
		 {
			if (Entity is Player)
			{
				return;
			}
			CheckForHostile();
            if (!Acted)
            {
                foreach (Action goal in Goals)
                {
                    goal();
                    if (Acted)
                    {
                        return;
                    }
                }
            }
            if (!Acted)
            {
                Entity.TryComponent<Minion>()?.Act();
            }
            if (!Acted)
            {
                Wander();
            }
			
		}
		
		public void Patrol(int x1, int y1, int z1)
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
			int d = (int) Tiles.QuickDistance(x, y, z, x1, y1, z1);
			if (d>=5)
			{	
				WalkToward(x1, y1, z1);
			} else if (d<=2) 
			{
				WalkAway(x1, y1, z1);
			} else {
				WalkRandom();
			}
		}
		
		public void Wander() {
			WalkRandom();
		}
//		public override void OnAddToEntity() {
//			base.OnAddToEntity();
//			// nothing for now
//		}
		public void WalkToward(int x1, int y1, int z1, bool useLast=false)
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;
            Movement m = Entity.GetComponent<Movement>();
            var path = Tiles.FindPath(m, x1, y1, z1, useLast: useLast);
			Coord? target = (path.Count>0) ? path.First.Value : (Coord?) null;
			//Coord? target = Tiles.FindPath(x, y, z, x1, y1, z1, useLast: useLast);
			if (target==null)
			{
				WalkRandom();
			} else {
                if (Target.Entity is Creature && Team!=null && Team.IsHostile((Creature) Target.Entity))
                {
                    if (Tiles.QuickDistance(Entity, Target)<=1)
                    {
                        Debug.WriteLine("Trying to attack!!!");
                    }
                }
				Coord t = (Coord) target;
				TryStepTo(t.X, t.Y, t.Z);
				if (!Acted)
				{
					WalkRandom();
				}
			}
		}
		public void WalkAway(int x1, int y1, int z1)
		{
			int x0 = Entity.X;
			int y0 = Entity.Y;
			int z0 = Entity.Z;
			List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
			if (line.Count<=1)
			{
				WalkRandom();
			} else
			{
				Movement m = Entity.GetComponent<Movement>();
				int x = line[0].X-x0;
				int y = line[0].Y-y0;
				int z = z0;
				bool acted = TryStepTo(x, y, z);
				if (!acted)
				{
					WalkRandom();
				}
			}
		}
		public void WalkRandom()
		{
			int r = Game.World.Random.Next(4);
			Coord d = Movement.Directions4[r];
			int x1 = Entity.X + d.X;
			int y1 = Entity.Y + d.Y;
			int z1 = Entity.Z + d.Z;
			bool acted = TryStepTo(x1, y1, z1);
			if (!acted)
			{
				Wait();
			}
		}
		
		
		public bool TryStepTo(int x1, int y1, int z1)
		{
			Movement m = Entity.GetComponent<Movement>();
			// okay what's up here?
			int x = x1-Entity.X;
			int y = y1-Entity.Y;
			int z = z1-Entity.Z;
			Coord c = new Coord(x, y, z);
			Creature cr;
			// we want to loop through the base
			Coord[][] fallbacks;
			// use fallbacks for ordinary, directional movement
			if (Movement.Fallbacks.ContainsKey(c))
			{
				fallbacks = Movement.Fallbacks[c];
			}
			else
			{
				// otherwise mock up an array with only one row
				fallbacks = new Coord[][] {new Coord[] {c}};
			}
			foreach (var row in fallbacks)
			{
				foreach (Coord dir in row)
				{
					x = dir.X+Entity.X;
					y = dir.Y+Entity.Y;
					z = dir.Z+Entity.Z;
					cr = Game.World.Creatures[x, y, z];
					if (m.CanPass(x, y, z))
					{
						m.StepTo(x, y, z);
						return true;
					}
					else if (cr!=null && m.CanMove(x, y, z) && Team.IsFriendly(cr) && cr!=Game.World.Player)
					{
						Minion minion = cr.TryComponent<Minion>();
						if (minion!=null && minion.Task!=null)
						{
							// for now, make working creatures unpushable
							// eventually, try to push creature into a neighboring square where it can still work
							
							continue;
						}
						if (Game.World.Random.NextDouble()<0.5)
						{
							m.Displace(cr);
							return true;
						}
					}
				}
			}
			return false;
		}
		public void Wait()
		{
			Spend(ActionPoints);
		}
		
		public void CheckForHostile()
		{
			int x = Entity.X;
			int y = Entity.Y;
			int z = Entity.Z;


            // so...back in the JS version, this totally flogged performance.
            // we could rebuild the hostility matrix every time a team changes...
            if (Target==null && Team!=null)
            {
                List<Creature> enemies;
                if (Team.Berserk)
                {
                    enemies = Game.World.Creatures.Where(cr => cr!=Entity).ToList();
                }
                else
                {
                    enemies = Team.GetEnemies();
                    //foreach (var enemy in enemies)
                    //{
                    //    Debug.Print("{0} found an enemy in {1}, {2} away.", Entity.Describe(), enemy.Describe(), Tiles.QuickDistance(Entity.X, Entity.Y, Entity.Z, enemy.X, enemy.Y, enemy.Z));
                    //}
                }
                enemies = enemies.Where(cr => (Tiles.QuickDistance(x, y, z, cr.X, cr.Y, cr.Z) < 10)).ToList();
                //enemies = enemies.OrderBy(cr => Tiles.QuickDistance(x, y, z, cr.X, cr.Y, cr.Z)).ToList();
                if (enemies.Count > 0)
                {
                    Debug.WriteLine("found an enemy");
                    Target = enemies[0];
                }
            }
            if (Target==Entity)
            {
                Debug.WriteLine("Somehow ended up targeting itself.");
            }
            if (Target != null && Target.Entity is Creature && Team.IsHostile((Creature) Target.Entity))
            {
                Movement m = Entity.GetComponent<Movement>();
                Attacker a = Entity.TryComponent<Attacker>();
                // this is poorly thought out
                if (m.CanTouch(Target.X, Target.Y, Target.Z) && a != null)
                {
                    Debug.WriteLine("attacking");
                    a.Attack(Target);
                }
                else
                {
                    WalkToward(Target.X, Target.Y, Target.Z);
                }
            }
        }
		

		public override void InterpretJSON(string json)
		{
			JObject obj = JObject.Parse(json);
			if (obj["TeamName"]!=null)
			{
                this.Team = Team.Types[(string)obj["TeamName"]];
			}
            if (obj["Goals"]!=null)
            {
                List<string> goals = obj["Goals"].ToObject<List<string>>();
                foreach (string s in goals)
                {
                    if (s=="HuntForPlayer")
                    {
                        Goals.Add(HuntForPlayer);
                    }
                }
            }
		}


        public void HuntForPlayer()
        {
            Debug.WriteLine("hunting for player");
            if (Target == null)
            {
                Movement m = Entity.GetComponent<Movement>();
                if (m.CanReach(Game.World.Player))
                {
                    Target = Game.World.Player;
                }
                else
                {
                    List<Feature> doors = Game.World.Features.Where(f => (f.TypeName == "Door")).ToList();
                    foreach (Feature door in doors)
                    {
                        if (m.CanReach(door))
                        {
                            Target = door;
                            break;
                        }
                    }
                }
            }
            //else if (Target==Game.World.Player || Target?.TypeName=="Door")
            //{
             //   WalkToward(Target.X, Target.Y, Target.Z);
            //}
        }
		
	}
}