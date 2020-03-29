using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    using static HecatombAliases;

    public class EncounterTracker : StateHandler
    {
        public int PastEncounters;
        public List<EntityField<Creature>> MyCreatures;
        //public TileEntityField<DummyTile> EntryTile;
        public Coord EntryTile;
        public int TurnsSince;
        public int Frustration;
        public int FrustrationLimit = 25;
        public bool FrustrationAnnounced;

        public virtual void Act(Creature c)
        {

        }


        // okay, so now this can no longer be just the player, it needs to be able to target doors
        public void TargetPlayer(Creature c)
        {
            Actor actor = c.GetComponent<Actor>();
            if (actor.Target == null)
            {
                Movement m = c.GetComponent<Movement>();
                if (m.CanReach(Player))
                {
                    actor.Target = Player;
                }
                else
                {
                    List<Feature> doors = Features.Where<Feature>((f) => (f.TypeName == "Door")).ToList();
                    doors = doors.OrderBy((f) => Tiles.QuickDistance(f.X, f.Y, f.Z, Player.X, Player.Y, Player.Z)).ToList();
                    foreach (var f in doors)
                    {
                        if (m.CanReach(f, useLast: false))
                        {
                            actor.Target = f;
                            return;
                        }
                    }
                }
            }
        }

        public void LeaveTheMap(Creature cr)
        {
            var actor = cr.GetComponent<Actor>();
            var (x, y, z) = EntryTile;
            // if you're right near where you entered the map, despawn
            if (Tiles.QuickDistance(cr.X, cr.Y, cr.Z, x, y, z) <= 1)
            {
                Debug.WriteLine("Screw you guys, I'm going home.");
                cr.Leave();
            }
            else
            {
                actor.WalkToward(x, y, z);
            }
        }

        public bool IsFrustrated(Creature cr)
        {
            if (Frustration > FrustrationLimit)
            {
                return true;
            }
            return false;
        }
    }

    class HumanTracker : EncounterTracker
    {
        
        

        public HumanTracker()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
            AddListener<AttackEvent>(OnBanditAttack);
            AddListener<ActEvent>(OnAct);
            //AddListener<DestroyEvent>(OnDestroy);
            MyCreatures = new List<EntityField<Creature>>();
        }


        //public GameEvent OnDestroy(GameEvent ge)
        //{
        //    if (MyCreatures.Count == 0)
        //    {
        //        return ge;
        //    }
        //    DestroyEvent de = (DestroyEvent)ge;
        //    if (de.Entity is Creature)
        //    {
        //        Creature cr = (Creature)de.Entity;
        //        if (MyCreatures.Contains(cr))
        //        {

        //        }
        //    }
        //    return ge;
        //}
        public GameEvent OnAct(GameEvent ge)
        {
            if (MyCreatures.Count == 0)
            {
                return ge;
            }
            // this is all preliminary
            ActEvent ae = (ActEvent)ge;
            Actor actor = ae.Actor;
            if (actor.Acted || !(actor.Entity.Unbox() is Creature))
            {
                return ge;
            }
            Creature cr = (Creature)actor.Entity.Unbox();
            if (!MyCreatures.Contains(cr))
            {
                return ge;
            }
            Movement m = cr.GetComponent<Movement>();
            Senses s = cr.GetComponent<Senses>();
            if (ae.Step == "BeforeAlert")
            {
                // this makes sense
                if (actor.Target == null && !IsFrustrated(cr))
                {
                    TargetPlayer(cr);
                }
            }
            else if (ae.Step == "BeforeVandalism")
            {
                // if the player were nearby and reachable we would have targeted them in the alert step
                if (IsFrustrated(cr) && (Tiles.QuickDistance(Player.X, Player.Y, Player.Z, cr.X, cr.Y, cr.Z) > s.Range || !m.CanReach(Player)))
                {
                    actor.Target = null;
                    if (!FrustrationAnnounced)
                    {
                        Game.SplashPanel.Splash(new List<ColoredText> { "Unable to penetrate your defenses, the bandits grow frustrated and break off the siege." });
                        FrustrationAnnounced = true;
                    }
                    LeaveTheMap(cr);
                }
            }
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            //Debug.WriteLine($"We've had {PastBanditAttacks} bandit attacks so far.");
            TurnsSince += 1;
            if (Options.NoHumanAttacks)
            {
                return ge;
            }
            if (TurnsSince > 1500 && Game.World.Random.Arbitrary(100,OwnSeed()) == 0)
            //if (TurnsSince > 1500 && Game.World.Random.Next(100) == 0)
            {             
                BanditAttack();
            }
            return ge;
        }

        public void BanditAttack(bool debugCloser = false)
        {
            FrustrationAnnounced = false;
            Frustration = 0;
            TurnsSince = 0;
            MyCreatures.Clear();
            bool xwall = (Game.World.Random.Arbitrary(2, OwnSeed()) == 0);
            bool zero = (Game.World.Random.Arbitrary(2, OwnSeed()+1) == 0);
            //bool xwall = (Game.World.Random.Next(2)==0);
            //bool zero = (Game.World.Random.Next(2) == 0);
            string dir = "";
            if (xwall)
            {
                dir = (zero) ? "western" : "eastern";
            }
            else
            {
                dir = (zero) ? "northern" : "southern";
            }
            Game.SplashPanel.Splash(new List<ColoredText> {
               "A gang of bandits has been spotted near the " + dir + " border of your domain.",
               " ",
                "They must be coming to loot your supplies.  You should either hide behind sturdy doors, or kill them and take their ill-gotten loot for your own."
            },
            logText: "{red}A gang of bandits approaches from the "+dir+" border!");
            int x0, y0;
            if (xwall)
            {
                x0 = (zero) ? 1 : Game.World.Width - 2;
                y0 = Game.World.Random.Arbitrary((Game.World.Height - 2),OwnSeed()) + 1;
                //y0 = Game.World.Random.Next(Game.World.Height - 2) + 1;
                if (debugCloser)
                {
                    x0 = (zero) ? 75 : 180;
                    y0 = Game.World.Random.Next(Game.World.Height - 75) + 75;
                }
            }
            else
            {
                y0 = (zero) ? 1 : Game.World.Height - 2;
                x0 = Game.World.Random.Arbitrary(Game.World.Width - 2, OwnSeed()+1) + 1;
                //x0 = Game.World.Random.Next(Game.World.Width - 2) + 1;
                if (debugCloser)
                {
                    y0 = (zero) ? 75 : 180;
                    x0 = Game.World.Random.Next(Game.World.Width - 75) + 75;
                }
            }
            // for repeatable testing
            //x0 = 12;
            //y0 = 12;

            EntryTile = new Coord(x0, y0, Game.World.GetGroundLevel(x0, y0));
            for (int i = 0; i < PastEncounters + 1; i++)
            {
                string creature = (i % 3 == 2) ? "WolfHound" : "HumanBandit";

                Coord? cc = Creature.FindPlace(x0, y0, 0);
                if (cc != null)
                {
                    Coord c = (Coord)cc;
                    var bandit = Entity.Spawn<Creature>(creature);
                    bandit.Place(c.X, c.Y, c.Z);
                    MyCreatures.Add(bandit);
                    TargetPlayer(bandit);
                    // do they occasionally get placed one step underground?
                    Debug.WriteLine($"{bandit.Describe()} placed at {bandit.X} {bandit.Y}");
                    if (i == 0)
                    {
                        Item loot = Item.SpawnNewResource("Gold", 1);
                        bandit.GetComponent<Inventory>().Item = loot;
                    }
                }
                
            }
            PastEncounters += 1;
        }
        
        public GameEvent OnBanditAttack(GameEvent ge)
        {
            AttackEvent ae = (AttackEvent)ge;
            Attacker att = ae.Attacker;
            Entity e = att.Entity.Unbox();
            if (!(e is Creature))
            {
                return ge;
            }
            Creature cr = (Creature) e; 
            if (MyCreatures.Contains(cr))
            {
                if (ae.Defender.Entity.Unbox().TypeName == "Door")
                {
                    Debug.WriteLine($"attacking a door, frustration {Frustration}");
                    Frustration += 1;
                }
            }
            // this should track the bandits' frustration with pounding on doors, etc?
            return ge;
        }

        //public GameEvent OnPathChange(GameEvent ge)
        //{
        //    foreach (Creature bandit in Bandits)
        //    {
        //        Actor actor = bandit.GetComponent<Actor>();
        //        Movement move = bandit.GetComponent<Movement>();
        //        if (Tiles.QuickDistance(bandit, Player)<=25 && move.CanReach(Player))
        //        {
        //            actor.Target = Player;
        //        }

        //    }
        //    return ge;
        //}
    }
}
