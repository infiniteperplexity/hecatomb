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
using System.Reflection;

namespace Hecatomb
{
    using static HecatombAliases;
	
	public class Actor : Component, ICallStrings
	{
        public TileEntityField<TileEntity> Target;
        public Coord? TargetTile;
		[JsonIgnore] public int ActionPoints;
		public int CurrentPoints;
		public string Team;
        public bool Acted;
        public bool DeclaredEnemy;
        public List<string> Goals = new List<string>();
        public string Alert;
        public string Fallback;
        public EntityField<EncounterTracker> EncounterTracker;
        public void CallString(string s)
        {
            Type thisType = this.GetType();
            MethodInfo theMethod = thisType.GetMethod(s);
            theMethod.Invoke(this, new object[0]);
        }
        // so I guess this doesn't get reconstituted correctly when restoring a game
		public Actor() : base()
		{
			ActionPoints = 16;
            CurrentPoints = (Turns.Turn==0) ? ActionPoints : 0;
            Alert = "CheckForHostile";
            Fallback = "WalkRandom";
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
        public void Spend() => Spend(16);

        public void Act()
        {
            if (Entity == Player)
            {
                return;
            }
            CallString(Alert);
            if (!Acted && EncounterTracker != null)
            {
                EncounterTracker.Entity.Act((Creature)Entity);
            }
            if (!Acted)
            {
                foreach (string goal in Goals)
                {
                    CallString(goal);
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
            if (!Acted && TargetTile != null)
            {
                // do we need to check whether it's on that tile?
                var (x, y, z) = (Coord)TargetTile;
                WalkToward(x, y, z);
            }
            if (!Acted)
            {
                Wander();
            }
			
		}
		
		public void Patrol(int x1, int y1, int z1)
		{
            var (x, y, z) = Entity;
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

        public void Wander() => WalkRandom();

        public bool IsHostile(Creature c)
        {
            return GetState<TeamHandler>().CheckHostile(Team, c);
        }
        public bool IsHostile(string team)
        {
            return GetState<TeamHandler>().CheckHostile(Team, team);
        }
        public bool IsFriendly(Creature c)
        {
            return IsFriendly(c.GetComponent<Actor>().Team);
        }
        public bool IsFriendly(string team)
        {
            return (Team == team);
        }


		public void WalkToward(int x1, int y1, int z1, bool useLast=false)
		{
            var (x, y, z) = Entity;
            Movement m = Entity.GetComponent<Movement>();
            var path = Tiles.FindPath(m, x1, y1, z1, useLast: useLast);
			Coord? target = (path.Count>0) ? path.First.Value : (Coord?) null;
			if (target==null)
			{
				WalkRandom();
			} else {
                if (Target?.Entity is Creature && IsHostile((Creature) Target.Entity))
                {
                    if (Tiles.QuickDistance(Entity, Target)<=1)
                    {
                        Debug.WriteLine("Trying to attack!!!");
                    }
                }
				Coord t = (Coord) target;
                if (Target?.Entity is Feature && Team!="Friendly" && (Target.Entity as Feature).Solid)
                {
                    if (Tiles.QuickDistance(Entity, Target) <= 1)
                    {
                        Debug.WriteLine("would like to attack the door at this point");
                        //WalkRandom();
                    }
                }
                if (!Acted)
                {
                    TryStepTo(t.X, t.Y, t.Z);
                }
				if (!Acted)
				{
					WalkRandom();
				}
			}
		}
		public void WalkAway(int x1, int y1, int z1)
		{
            var (x0, y0, z0) = Entity;
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
					cr = Creatures[x, y, z];
					if (m.CanPass(x, y, z))
					{
						m.StepTo(x, y, z);
						return true;
					}
					else if (cr!=null && m.CanMove(x, y, z) && IsFriendly(cr) && cr!=Player)
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
        public void Wait() => Spend(ActionPoints);
		
		public void CheckForHostile()
		{
            var (x, y, z) = Entity;
            // so...back in the JS version, this totally flogged performance.
            // we could rebuild the hostility matrix every time a team changes...
            if (Target==null && Team!=null)
            {
                List<Creature> enemies = GetState<TeamHandler>().GetEnemies(Entity.Entity as Creature);
                enemies = enemies.Where(cr => (Tiles.QuickDistance(x, y, z, cr.X, cr.Y, cr.Z) < 10)).ToList();
                if (enemies.Count > 0)
                {
                    Target = enemies[0];
                }
            }
            if (Target==Entity)
            {
                Debug.WriteLine("Somehow ended up targeting itself.");
            }
            if (Target != null && Target.Entity is Creature && IsHostile((Creature)Target.Entity))
            {
                Creature cr = (Creature)Target;
                Movement m = Entity.GetComponent<Movement>();
                Attacker a = Entity.TryComponent<Attacker>();
                // this is poorly thought out
                if (m.CanTouch(Target.X, Target.Y, Target.Z) && a != null)
                {
                    a.Attack(cr);
                }
                else
                {
                    WalkToward(Target.X, Target.Y, Target.Z);
                }
            }
            // not sure if this is exactly what we want to do
            else if (Target != null && Target.Entity is Feature)
            {
                Feature fr = (Feature)Target;
                Movement m = Entity.GetComponent<Movement>();
                Attacker a = Entity.TryComponent<Attacker>();
                Defender d = fr.TryComponent<Defender>();

                // this is poorly thought out
                if (m.CanTouch(Target.X, Target.Y, Target.Z) && a != null && d!=null)
                {
                    Debug.WriteLine("Attacking a door");
                    a.Attack(fr);
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
			if (obj["Team"]!=null)
			{
                this.Team = (string)obj["Team"];
			}
            if (obj["Goals"] != null)
            {
                List<string> goals = obj["Goals"].ToObject<List<string>>();
                foreach (string s in goals)
                {
                    if (s == "HuntForPlayer")
                    {
                        Goals.Add("HuntForPlayer");
                    }
                }
            }
        }

        public void Provoke(Creature c)
        {
            if (c.GetComponent<Actor>().Team=="Friendly")
            {
                Team = "Hostile";
            }
            if (IsHostile(c))
            {
                if (Target == null)
                {
                    Target = c;
                }
                else if (Tiles.QuickDistance(Entity, c)<Tiles.QuickDistance(Entity, Target))
                {
                    Target = c;
                }
            }
        }

        
		
	}
}