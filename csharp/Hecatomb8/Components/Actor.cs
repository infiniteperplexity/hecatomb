using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Hecatomb8
{
    using static HecatombAliases;

    public partial class Activity
    {
        public static readonly Activity Alert = new Activity(
            type: "Alert",
            act: Actor._alert
        );

        public static readonly Activity Seek = new Activity(
            type: "Seek",
            act: Actor._seek
        );

        public static readonly Activity Wander = new Activity(
            type: "Wander",
            act: (a, cr) => a.Wander()
        );

        public static readonly Activity Default = new Activity(
            type: "Default",
            act: (a, cr) => {
                if (!a.Acted)
                {
                    Alert.Act(a, cr);
                }
                if (!a.Acted)
                { 
                    Seek.Act(a, cr);
                }
                if (!a.Acted)
                {
                    Wander.Act(a, cr);
                }
            }
        );

        public static readonly Activity TargetPlayer = new Activity(
            type: "TargetPlayer",
            act: Actor._targetPlayer
        );
    }


    public class Actor : Component
    {
        public static int DefaultPoints = 16;
        public ListenerHandledEntityHandle<TileEntity>? Target;
        [JsonIgnore] public int ActionPoints;
        public int CurrentPoints;
        public Team Team;
        public bool Acted;
        public bool Active;
        public Func<Creature, bool>? TestLogic;
        public List<Activity> Activities;
        [JsonIgnore]
        Movement? cachedMovement;
        [JsonIgnore]
        Movement? Movement
        {
            get
            {
                if (cachedMovement != null && cachedMovement.Spawned)
                {
                    return cachedMovement;
                }
                else
                {
                    if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Spawned)
                    {
                        return null;
                    }
                    cachedMovement = Entity.UnboxBriefly()!.GetComponent<Movement>();
                    return cachedMovement;
                }
            }
        }

        // so I guess this doesn't get reconstituted correctly when restoring a game?
        public Actor() : base()
        {
            Team = Team.Neutral;
            Active = true;
            ActionPoints = DefaultPoints;
            CurrentPoints = (GetState<TurnHandler>().Turn == 0) ? ActionPoints : 0;
            Activities = new List<Activity>(){};
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity == Target?.UnboxBriefly())
            {
                Target = null;
            }
            return base.OnDespawn(ge);
        }

        public void SetTarget(TileEntity t)
        {
            if (Entity?.UnboxBriefly() == t)
            {
                Debug.WriteLine("why did I try to target myself?");
                return;
            }
            Target = t.GetHandle<TileEntity>(OnDespawn);
        }

        public void Regain()
        {
            while (CurrentPoints <= 0)
            {
                CurrentPoints += DefaultPoints;
            }
            Acted = false;
        }
        public void Spend(int i)
        {
            CurrentPoints -= i;
            Acted = true;
        }
        public void Spend() => Spend(DefaultPoints);


        public void Act()
        {
            if (!Active)
            {
                Spend();
                return;
            }
            var entity = Entity?.UnboxBriefly();
            if (entity is null || !entity.Spawned || !entity.Placed)
            {
                return;
            }
            Creature cr = (Creature)entity;
            if (cr == Player)
            {
                return;
            }
            foreach (Activity activity in Activities)
            {
                if (!Acted)
                {
                    activity.Act(this, cr);
                }
            }
            if (!Acted)
            {
                Wait();
            }
        }

        public void Patrol(TileEntity t)
        {
            if (t.Placed)
            {
                Patrol((int)t.X!, (int)t.Y!, (int)t.Z!);
            }
        }

        public void Patrol(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x, y, z) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            int d = (int)Tiles.Distance((int)x!, (int)y!, (int)z!, x1, y1, z1);
            if (d >= 5)
            {
                WalkToward(x1, y1, z1);
            }
            else if (d <= 2)
            {
                WalkAway(x1, y1, z1);
            }
            else
            {
                WalkRandom();
            }
        }

        public void Wander() => WalkRandom();

        public bool IsHostile(Team team)
        {
            return Team.Enemies.Contains(team);
        }

        public bool IsHostile(ComposedEntity ce)
        {
            if (ce is Creature)
            {
                return Team.Enemies.Contains((ce as Creature)!.GetComponent<Actor>().Team);
            }
            else if (ce is Feature)
            {
                if (/*f.Owned && */(ce as Feature)!.HasComponent<Defender>() && IsHostile(Player))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsFriendly(Creature c)
        {
            return IsFriendly(c.GetComponent<Actor>().Team);
        }
        public bool IsFriendly(Team team)
        {
            return (Team == team);
        }

        public void WalkToward(TileEntity t, bool useLast = false, int vagueDistance = 25)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed || !t.Placed)
            {
                return;
            }
            Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
            LinkedList<Coord> path = Tiles.FindPath(m, t, useLast: useLast, movable: m.CouldMoveBounded, standable: m.CanStandBounded);
            if (path.Count == 0)
            {
                Wander();
            }
            else
            {
                Coord target = path.First!.Value;
                TryStepTo(target.X, target.Y, target.Z);
            }
        }

        public void WalkToward(int x1, int y1, int z1, bool useLast = false, TileEntity? targetEntity = null, int vagueDistance = 25)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x, y, z) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            if (Tiles.Distance(x, y, z, x1, y1, z1) > vagueDistance)
            {

                WalkVaguelyToward(x1, y1, z1);
                return;
            }
            Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
            LinkedList<Coord> path;
            if (targetEntity is null || !targetEntity.Spawned || !targetEntity.Placed)
            {
                path = Tiles.FindPath(m, x1, y1, z1, useLast: useLast, movable: m.CouldMoveBounded, standable: m.CanStandBounded);
            }
            else
            {
                path = Tiles.FindPath(m, targetEntity, useLast: useLast, movable: m.CouldMoveBounded, standable: m.CanStandBounded);
            }
            Coord? target = (path.Count > 0) ? path.First!.Value : (Coord?)null;
            if (target == null)
            {
                // this is what we do if we can't reach our target
                WalkRandom();
            }
            else
            {
                // wait what the crap????
                // so, I have the following note from some time in the past:
                // this way of doing it makes it hard to attack things that are in the way...
                // why on earth would I be doing it this way?
                // well...if the creature is standing *in* a doorway, this would let us attack
                // huh
                // but probably I should have attacked earlier in this process, right?
                // I'm going to comment this sketchy crap out for now
                //if (Target?.Entity is Creature && IsHostile((Creature)Target.Entity))
                //{
                //    if (m.CanTouch(Target.X, Target.Y, Target.Z))
                //    {
                //        Entity.TryComponent<Attacker>().Attack(Target.Unbox() as Creature);
                //    }
                //}
                //else if (Target?.Entity is Feature && IsHostile((Feature)Target.Entity))
                //{
                //    if (m.CanTouch(Target.X, Target.Y, Target.Z))
                //    {
                //        Entity.TryComponent<Attacker>().Attack(Target.Unbox() as Feature);
                //    }
                //}
                Coord t = (Coord)target;
                if (!Acted)
                {
                    TryStepTo(t.X, t.Y, t.Z);
                }
                if (!Acted)
                {
                    WalkRandom();
                }
            }
            if (!Acted)
            {
                WalkRandom();
            }
        }


        public void WalkVaguelyToward(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x0, y0, z0) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
            if (line.Count <= 1)
            {
                WalkRandom();
            }
            else
            {
                Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
                int x = line[1].X;
                int y = line[1].Y;
                int z = z0;
                bool acted = TryStepTo(x, y, z);
                if (!acted)
                {
                    WalkRandom();
                }
            }
        }


        public void WalkAway(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x0, y0, z0) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            List<Coord> line = Tiles.GetLine((int)x0!, (int)y0!, x1, y1);
            if (line.Count <= 1)
            {
                WalkRandom();
            }
            else
            {
                Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
                //Movement m = CachedMovement;
                int x = line[0].X - (int)x0!;
                int y = line[0].Y - (int)y0!;
                int z = (int)z0!;
                bool acted = TryStepTo(x, y, z);
                if (!acted)
                {
                    WalkRandom();
                }
            }
        }
        public void WalkRandom()
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            //int r = GameState.World!.Random.Arbitrary(4, OwnSeed());
            int r = GameState.World!.Random.Next(4);

            Coord d = Coord.Directions4[r];
            bool acted = false;
            var (x, y, z) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            int x1 = (int)x! + d.X;
            int y1 = (int)y! + d.Y;
            int z1 = (int)z! + d.Z;
            acted = TryStepTo(x1, y1, z1);
            if (!acted)
            {
                Wait();
            }
        }


        public bool TryStepTo(int x1, int y1, int z1)
        {
            if (Movement is null || Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            var (_x, _y, _z) = Entity.UnboxBriefly()!.GetPlacedCoordinate();
            int x = x1 - (int)_x!;
            int y = y1 - (int)_y!;
            int z = z1 - (int)_z!;
            if (x == 0 && y == 0 && z == 0)
            {
                Debug.WriteLine($"{Entity.UnboxBriefly()!.Describe()} tried to step on itself for some reason.");
            }
            Coord c = new Coord(x, y, z);
            Creature? cr;
            Feature? fr;
            // we want to loop through the base
            Coord[][] fallbacks;
            // use fallbacks for ordinary, directional movement
            if (Movement.Fallbacks.ContainsKey(c))
            {
                fallbacks = Movement.Fallbacks[c];
            }
            else
            {
                // I don't understand why we get here sometimes...perhaps displacement?
                fallbacks = new Coord[][] { new Coord[] { c } };
            }
            foreach (var row in fallbacks)
            {
                foreach (Coord dir in row)
                {
                    // this void tiles should prevent this from going out of bounds
                    x = dir.X + (int)_x!;
                    y = dir.Y + (int)_y!;
                    z = dir.Z + (int)_z!;
                    cr = Creatures.GetWithBoundsChecked(x, y, z);
                    // I don't think this should ever happen but if it does just skip around it
                    if (cr == Entity.UnboxBriefly())
                    {
                        continue;
                    }
                    fr = Features.GetWithBoundsChecked(x, y, z);
                    // if there's nothing in the way
                    if (Movement.CanPassBounded(x, y, z))
                    {
                        Movement.StepToValidEmptyTile(x, y, z);
                        return true;
                    }
                    // this is essentially about dealing with stuff that's 
                    else if (cr != null && Movement.CanMoveBounded(x, y, z))
                    {
                        if (IsFriendly(cr) && cr != Player)
                        {
                            if (cr.HasComponent<Minion>())
                            {
                                Minion minion = cr.GetComponent<Minion>();
                                Task? task = minion!.Task?.UnboxBriefly();
                                // we should be reluctant to displace a friendly creature if it is working on a task
                                if (task != null && task.CanWork())
                                {
                                    // intuitively, it seems like we should first check if normal displacement works
                                    // but in practice it looks really odd and jarring that way
                                    // instead we first try pushing the creature, then we try displacing it normally
                                    var squares = Tiles.GetNeighbors26(x, y, z, where: (xd, yd, zd) =>
                                    {
                                        // gather all squares you could displace the creature to and still have it work
                                        return (task.CouldWorkFrom(xd, yd, zd) & Movement.CanPassBounded(xd, yd, zd));
                                    });
                                    if (squares.Count > 0)
                                    {
                                        // choose a square randomly from that list
                                        int r = GameState.World!.Random.Next(squares.Count);
                                        var s = squares[r];
                                        // push the friendly creature onto that square
                                        Movement.Displace(cr, s.X, s.Y, s.Z);
                                        return true;
                                    }
                                    // so...I think now we displace it only if it can still work...
                                    // ...otherwise you could get two guys in a dead end shoving each out of the way repeatedly
                                    else if (task.CouldWorkFrom(_x, _y, _z))
                                    {
                                        Movement.Displace(cr);
                                        return true;
                                    }
                                }


                            }
                            // wait...why do we only do it 50% of the time?  can we get loops othewise?
                            // let's try 100% to see what happens
                            if (GameState.World!.Random.Next(4) == 0)
                            {
                                Movement.Displace(cr);
                                return true;
                            }
                        }
                        else if (IsHostile(cr))
                        {
                            if (Entity.UnboxBriefly()!.HasComponent<Attacker>())
                            {
                                Attacker a = Entity.UnboxBriefly()!.GetComponent<Attacker>();
                                a.Attack(cr);
                            }

                        }
                    }
                    else if (fr != null && IsHostile(fr))
                    {
                        if (Entity.UnboxBriefly()!.HasComponent<Attacker>())
                        {
                            Attacker a = Entity.UnboxBriefly()!.GetComponent<Attacker>();
                            a.Attack(fr);
                        }
                    }
                }
            }
            return false;
        }

        public void Wait() => Spend(ActionPoints);


        private bool reachableHostileCreature(int x, int y, int z)
        {
            // we might look to skip this step for performance purposes
            if (Movement is null)
            {
                return false;
            }
            Creature? cr = Creatures.GetWithBoundsChecked(x, y, z);
            if (cr != null && cr.GetComponent<Actor>().Active && IsHostile(cr) && Movement.CanReachBounded(cr))
            {
                return true;
            }
            return false;
        }

        private bool reachableEnemyFeature(int x, int y, int z)
        {
            // we might look to skip this step for performance purposes
            if (Movement is null)
            {
                return false;
            }
            Feature? fr = Features.GetWithBoundsChecked(x, y, z);
            if (fr != null && IsHostile(fr) & Movement.CanReachBounded(fr, useLast: false))
            {
                return true;
            }
            return false;
        }



        public static void _alert(Actor a, Creature cr)
        {
            var (x, y, z) = cr.GetPlacedCoordinate();
            // let's try not prioritizing creature targets unless they attack us
            if (a.Team != Team.Neutral && a.Target?.UnboxBriefly() is null )
            //if (a.Team != Team.Neutral && (a.Target?.UnboxBriefly() is null || a.Target.UnboxBriefly() is Feature))
            {
                Coord? c;
                c = cr.GetComponent<Senses>().FindClosestVisible(where: a.reachableHostileCreature);
                if (c != null)
                {
                    Coord C = (Coord)c;
                    a.SetTarget(Creatures.GetWithBoundsChecked(C.X, C.Y, C.Z)!);
                }
            }
        }

        public static void _seek(Actor a, Creature cr)
        {
            //
            
            // this was a discrete chunk of logic that acquires targets
            var target = a.Target?.UnboxBriefly();
            if (target != null && target.Placed && a.IsHostile((ComposedEntity)target))
            {
                ComposedEntity crt = (ComposedEntity)target;
                Movement m = cr.GetComponent<Movement>();
                Attacker attacker = cr.GetComponent<Attacker>();
                var distance = Tiles.Distance(cr, target);
                if (distance < 25 && !m.CanReachBounded(crt, useLast: false))
                {
                    a.Target = null;
                }
                else if (m.CanTouchBounded((int)crt.X!, (int)crt.Y!, (int)crt.Z!))
                {
                    attacker.Attack(crt);
                }
                else
                {
                    a!.WalkToward(crt);
                }
            }
        }

        public static void _vandalize(Actor a, Creature cr)
        {
            var (x, y, z) = cr.GetPlacedCoordinate();
            // definitely don't untarget features to prioritize other features
            
            if (a.Team != Team.Neutral && a.Target?.UnboxBriefly() is null)
            {
                Coord? c;
                c = cr.GetComponent<Senses>().FindClosestVisible(where: a.reachableEnemyFeature);
                if (c != null)
                {
                    Coord C = (Coord)c;
                    a.SetTarget(Features.GetWithBoundsChecked(C.X, C.Y, C.Z)!);
                }
            }
        }

        public static void _targetPlayer(Actor actor, Creature cr)
        {
            if (actor.Target is null)
            {
                Movement m = cr.GetComponent<Movement>();

                if (Tiles.Distance(cr, Player) > 30 || m.CanReachBounded(Player))
                {
                    actor.Target = Player.GetHandle<TileEntity>(actor.OnDespawn);
                }
                //else
                //{
                //    List<Feature> doors = Features.Where<Feature>((f) => (f is Door)).ToList();
                //    doors = doors.OrderBy((f) => Tiles.Distance((int)f.X!, (int)f.Y!, (int)f.Z!, (int)Player.X!, (int)Player.Y!, (int)Player.Z!)).ToList();
                //    foreach (var f in doors)
                //    {
                //        if (m.CanReachBounded(f, useLast: false))
                //        {
                //            actor.Target = f.GetHandle<TileEntity>(actor.OnDespawn);
                //            return;
                //        }
                //    }
                //}
            }
        }

        public void ProvokeAgainst(Creature c)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed || !c.Spawned || !c.Placed)
            {
                return;
            }
            if ((Team == Team.Friendly || Team == Team.Neutral) && c.GetComponent<Actor>().Team == Team.Friendly)
            {
                Team = Team.Hostile;
            }
            if (IsHostile(c))
            {
                if (Target?.UnboxBriefly() is null || !Target.UnboxBriefly()!.Placed)
                {
                    SetTarget(c);
                }
                else if (Tiles.Distance(Entity.UnboxBriefly()!, c) < Tiles.Distance(Entity.UnboxBriefly()!, Target.UnboxBriefly()!))
                {
                    SetTarget(c);
                }
            }
        }
    }
}