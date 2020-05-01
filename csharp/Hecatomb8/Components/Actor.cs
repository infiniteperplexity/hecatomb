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

    public class Actor : Component
    {
        public static int DefaultPoints = 16;
        public ListenerHandledEntityHandle<TileEntity>? Target;
        [JsonIgnore] public int ActionPoints;
        public int CurrentPoints;
        //public Teams Team;
        public bool Acted;
        public bool Active;
        //[JsonIgnore]
        //Movement cachedMovement;
        //[JsonIgnore]
        //Movement CachedMovement
        //{
        //    get
        //    {
        //        if (cachedMovement == null)
        //        {
        //            cachedMovement = Entity.Unbox().GetComponent<Movement>();
        //        }
        //        return cachedMovement;
        //    }
        //}

        // so I guess this doesn't get reconstituted correctly when restoring a game?
        public Actor() : base()
        {
            Active = true;
            ActionPoints = DefaultPoints;
            CurrentPoints = (GetState<TurnHandler>().Turn == 0) ? ActionPoints : 0;
        }

        private void SetTarget(TileEntity t)
        {
            Target = t.GetHandle<TileEntity>(OnDespawn);
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            if (ge is DespawnEvent)
            {
                var de = (DespawnEvent)ge;
                if (Target != null && de.Entity == Target.UnboxBriefly())
                {
                    Target = null;
                }
            }
            base.OnDespawn(ge);
            return ge;
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
            if (cr.HasComponent<Minion>())
            {
                cr.GetComponent<Minion>().Act();
            }

            //Game.World.Events.Publish(new ActEvent() { Actor = this, Entity = Entity, Step = "BeforeAlert" });

            //if (!Acted)
            //{
            //    Alert();
            //}

            //Game.World.Events.Publish(new ActEvent() { Actor = this, Entity = Entity, Step = "BeforeVandalism" });

            //if (!Acted)
            //{
            //    Vandalize();
            //}


            //Game.World.Events.Publish(new ActEvent() { Actor = this, Entity = Entity, Step = "BeforeWander" });

            if (!Acted)
            {
                Wander();
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
            var (x, y, z) = Entity.UnboxBriefly()!.GetVerifiedCoord();
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

        //public bool IsHostile(Creature c)
        //{
        //    return GetState<TeamHandler>().CheckHostile(Team, c);
        //}
        //public bool IsHostile(Teams team)
        //{
        //    return GetState<TeamHandler>().CheckHostile(Team, team);
        //}
        //public bool IsHostile(Feature f)
        //{
        //    if (/*f.Owned && */f.TryComponent<Defender>() != null && IsHostile(Player))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public bool IsFriendly(Creature c)
        //{
        //    return IsFriendly(c.GetComponent<Actor>().Team);
        //}
        //public bool IsFriendly(Teams team)
        //{
        //    return (Team == team);
        //}
        
        public void WalkToward(TileEntity t, bool useLast = false)
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

        public void WalkToward(int x, int y, int z, bool useLast = false)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
            LinkedList<Coord> path = Tiles.FindPath(m, x, y, z, useLast: useLast, movable: m.CouldMoveBounded, standable: m.CanStandBounded);
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

        //public void WalkToward(TileEntity t, bool useLast = false, int vagueDistance = 50)
        //{
        //    var (x, y, z) = Entity;
        //    if (Tiles.QuickDistance(x, y, z, t.X, t.Y, t.Z) > vagueDistance)
        //    {
        //        WalkVaguelyToward(t.X, t.Y, t.Z);
        //    }
        //    else
        //    {
        //        WalkToward(t.X, t.Y, t.Z, useLast: useLast, targetEntity: t);
        //    }
        //}

        //public void WalkToward(int x1, int y1, int z1, bool useLast = false, TileEntity targetEntity = null, int vagueDistance = 50)
        //{
        //    var (x, y, z) = Entity;
        //    if (Tiles.QuickDistance(x, y, z, x1, y1, z1) > vagueDistance)
        //    {

        //        WalkVaguelyToward(x1, y1, z1);
        //        return;
        //    }
        //    Movement m = Entity.GetComponent<Movement>();
        //    LinkedList<Coord> path;
        //    if (targetEntity == null)
        //    {
        //        path = Tiles.FindPath(m, x1, y1, z1, useLast: useLast, movable: m.CouldMove, standable: m.CanStand);
        //    }
        //    else
        //    {
        //        path = Tiles.FindPath(m, targetEntity, useLast: useLast, movable: m.CouldMove, standable: m.CanStand);
        //    }
        //    Coord? target = (path.Count > 0) ? path.First.Value : (Coord?)null;
        //    if (target == null)
        //    {
        //        // this is what we do if we can't reach our target
        //        WalkRandom();
        //    }
        //    else
        //    {
        //        // this way of doing it makes it hard to attack things that are in the way...
        //        if (Target?.Entity is Creature && IsHostile((Creature)Target.Entity))
        //        {
        //            if (m.CanTouch(Target.X, Target.Y, Target.Z))
        //            {
        //                Entity.TryComponent<Attacker>().Attack(Target.Unbox() as Creature);
        //            }
        //        }
        //        else if (Target?.Entity is Feature && IsHostile((Feature)Target.Entity))
        //        {
        //            if (m.CanTouch(Target.X, Target.Y, Target.Z))
        //            {
        //                Entity.TryComponent<Attacker>().Attack(Target.Unbox() as Feature);
        //            }
        //        }
        //        Coord t = (Coord)target;
        //        if (!Acted)
        //        {
        //            TryStepTo(t.X, t.Y, t.Z);
        //        }
        //        if (!Acted)
        //        {
        //            WalkRandom();
        //        }
        //    }
        //    if (!Acted)
        //    {
        //        WalkRandom();
        //    }
        //}


        //public void WalkVaguelyToward(int x1, int y1, int z1)
        //{
        //    var (x0, y0, z0) = Entity;
        //    List<Coord> line = Tiles.GetLine(x0, y0, x1, y1);
        //    if (line.Count <= 1)
        //    {
        //        WalkRandom();
        //    }
        //    else
        //    {
        //        Movement m = Entity.GetComponent<Movement>();
        //        int x = line[1].X;
        //        int y = line[1].Y;
        //        int z = z0;
        //        bool acted = TryStepTo(x, y, z);
        //        if (!acted)
        //        {
        //            WalkRandom();
        //        }
        //    }
        //}


        public void WalkAway(int x1, int y1, int z1)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            var (x0, y0, z0) = Entity.UnboxBriefly()!.GetVerifiedCoord();
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
            var (x, y, z) = Entity.UnboxBriefly()!.GetVerifiedCoord();
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
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return false;
            }
            Movement m = Entity.UnboxBriefly()!.GetComponent<Movement>();
            // okay what's up here?
            var (_x, _y, _z) = Entity.UnboxBriefly()!.GetVerifiedCoord();
            int x = x1 - (int)_x!;
            int y = y1 - (int)_y!;
            int z = z1 - (int)_z!;
            if (x == 0 && y == 0 && z == 0)
            {
                Debug.WriteLine($"{Entity.UnboxBriefly()!.Describe()} tried to step on itself for some reason.");
            }
            Coord c = new Coord(x, y, z);
            Creature cr;
            //Feature fr;
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
                fallbacks = new Coord[][] { new Coord[] { c } };
            }
            foreach (var row in fallbacks)
            {
                foreach (Coord dir in row)
                {
                    // this void tiles shoudl prevent this from going out of bounds
                    x = dir.X + (int)_x!;
                    y = dir.Y + (int)_y!;
                    z = dir.Z + (int)_z!;
                    cr = Creatures.GetWithBoundsChecked(x, y, z)!;
                    //fr = Features[x, y, z];
                    // so...this logic is weird...they will route around hostile creatures rather than attacking them
                    if (m.CanPassBounded(x, y, z))
                    {
                        m.StepToValidEmptyTile(x, y, z);
                        return true;
                    }
                    // it *will* displace friendly creatures....
                    else if (cr != null && m.CanMoveBounded(x, y, z))
                    {
                        //if (IsFriendly(cr) && cr != Player)
                        //{
                        //    Minion minion = cr.TryComponent<Minion>();
                        //    if (minion != null && minion.Task != null)
                        //    {
                        //        Movement move = CachedMovement;
                        //        Task task = minion.Task;
                        //        // reluctant to displace a creature if it is standing next to its task
                        //        if (task.CanWork())
                        //        {

                        //            // note that it may seem intuitive to try swapping first, in practice it looks really odd and jarring
                        //            var squares = Tiles.GetNeighbors26(cr.X, cr.Y, cr.Z, where: (xd, yd, zd) =>
                        //            {
                        //                // gather all squares you could displace the creature to and still have it work
                        //                return (task.CouldWorkFrom(xd, yd, zd) & move.CanPass(xd, yd, zd));
                        //            });
                        //            if (squares.Count > 0)
                        //            {
                        //                // choose a square randomly from that list
                        //                int r = Game.World.Random.Arbitrary(squares.Count, OwnSeed() + 1);
                        //                //int r = Game.World.Random.Next(squares.Count);
                        //                var s = squares[r];
                        //                move.Displace(cr, s.X, s.Y, s.Z);
                        //                return true;
                        //            }
                        //            else if (task.CouldWorkFrom(Entity.X, Entity.Y, Entity.Z))
                        //            {
                        //                // should we do this anyway?  arguable...
                        //                move.Displace(cr);
                        //                return true;
                        //            }
                        //        }

                        //    }
                        //    //if (Game.World.Random.NextDouble() < 0.5)
                        //    if (Game.World.Random.Arbitrary(OwnSeed() + 2) < 0.5)
                        //    {
                        //        m.Displace(cr);
                        //        return true;
                        //    }
                        //}
                        //else if (IsHostile(cr))
                        //{
                        //    Attacker a = Entity.GetComponent<Attacker>();
                        //    a.Attack(cr);
                        //}
                    }
                    //else if (fr != null && IsHostile(fr))
                    //{
                    //    Attacker a = Entity.GetComponent<Attacker>();
                    //    a.Attack(fr);
                    //}
                }
            }
            return false;
        }

        public void SeekTarget()
        {

        }
        public void Wait() => Spend(ActionPoints);


        //private bool reachableHostileCreature(int x, int y, int z)
        //{
        //    Creature cr = Creatures[x, y, z];
        //    if (cr != null && IsHostile(cr) && !cr.GetComponent<Actor>().Inactive && CachedMovement.CanReach(cr))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool reachableEnemyFeature(int x, int y, int z)
        //{
        //    Feature fr = Features[x, y, z];
        //    if (fr != null && IsHostile(fr) && CachedMovement.CanReach(fr, useLast: false))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public void Alert()
        //{
        //    if (Acted)
        //    {
        //        return;
        //    }
        //    // we have no handling for being unable to reach the target!
        //    var (x, y, z) = Entity;
        //    if (Team != Teams.Neutral && (Target == null || Target.Unbox() is Feature))
        //    {
        //        Coord c;
        //        c = Entity.GetComponent<Senses>().FindClosestVisible(where: reachableHostileCreature);
        //        if (!c.Equals(default(Coord)))
        //        {
        //            Target = Creatures[c];
        //        }
        //    }
        //    if (Target == Entity)
        //    {
        //        Debug.WriteLine("Somehow ended up targeting itself.");
        //    }
        //    // we don't verify that we can reach it before trying
        //    // So...there's currently duplicate code for creatures and features...could be generalized
        //    if (Target != null && Target.Entity is Creature && IsHostile((Creature)Target.Entity))
        //    {
        //        Creature cr = (Creature)Target;
        //        Movement m = Entity.GetComponent<Movement>();
        //        Attacker a = Entity.TryComponent<Attacker>();
        //        if (!m.CanReach(Target))
        //        {
        //            Target = null;
        //        }
        //        // this is poorly thought out
        //        else if (m.CanTouch(Target.X, Target.Y, Target.Z) && a != null)
        //        {
        //            a.Attack(cr);
        //        }
        //        else
        //        {
        //            // doesn't matter that this can't cache misses; a miss at this point would throw and error
        //            WalkToward(cr);
        //        }
        //    }
        //}

        //public void Vandalize()
        //{
        //    if (Acted)
        //    {
        //        return;
        //    }
        //    // we have no handling for being unable to reach the target!
        //    var (x, y, z) = Entity;
        //    if (Target == null && Team != Teams.Neutral)
        //    {
        //        Coord c;
        //        c = Entity.GetComponent<Senses>().FindClosestVisible(where: reachableEnemyFeature);
        //        if (!c.Equals(default(Coord)))
        //        {
        //            Target = Features[c];
        //        }
        //    }
        //    // we don't verify that we can reach it before trying
        //    // So...there's currently duplicate code for creatures and features...could be generalized
        //    if (Target != null && Target.Entity is Feature && IsHostile((Feature)Target.Entity))
        //    {
        //        Feature fr = (Feature)Target;
        //        Movement m = Entity.GetComponent<Movement>();
        //        Attacker a = Entity.TryComponent<Attacker>();
        //        // this is poorly thought out
        //        if (!m.CanReach(Target, useLast: false))
        //        {
        //            Target = null;
        //        }
        //        else if (m.CanTouch(Target.X, Target.Y, Target.Z) && a != null)
        //        {
        //            a.Attack(fr);
        //        }
        //        else
        //        {
        //            // doesn't matter that this can't cache misses; a miss at this point would throw and error
        //            WalkToward(Target, useLast: false);
        //        }
        //    }
        //}

        //public void Provoke(Creature c)
        //{
        //    if (c.GetComponent<Actor>().Team == Teams.Friendly)
        //    {
        //        Team = Teams.Hostile;
        //    }
        //    if (IsHostile(c))
        //    {
        //        if (Target == null)
        //        {
        //            Target = c;
        //        }
        //        else if (Tiles.QuickDistance(Entity, c) < Tiles.QuickDistance(Entity, Target))
        //        {
        //            Target = c;
        //        }
        //    }
        //}
    }
}