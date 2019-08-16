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
        //public TileEntityField<DummyTile> EntryTile;
        public Coord EntryTile;
        public int TurnsSince;

        public virtual void Act(Creature c)
        {

        }


        public void TargetPlayer(Creature c)
        {
            Actor actor = c.GetComponent<Actor>();
            if (actor.Target == null)
            {
                Movement m = c.GetComponent<Movement>();
                if (m.CanReach(Player, ignoreDoors: true))
                {
                    actor.Target = Player;
                }
            }
        }
    }

    class HumanTracker : EncounterTracker
    {
        public int PastBanditAttacks;
        public List<EntityField<Creature>> Bandits;
        public int Frustration;
        public int FrustrationLimit = 25;

        public HumanTracker()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
            AddListener<AttackEvent>(OnBanditAttack);
            AddListener<ActEvent>(OnAct);
            Bandits = new List<EntityField<Creature>>();
        }

        public GameEvent OnAct(GameEvent ge)
        {
            ActEvent ae = (ActEvent)ge;
            Actor actor = ae.Actor;
            if (actor.Entity.Unbox() is Creature)
            {
;               Creature cr = (Creature)actor.Entity.Unbox();
                Movement m = cr.GetComponent<Movement>();
                if (Bandits.Contains(cr))
                {
                    if (!actor.Acted)
                    {
                        actor.Alert();
                    }
                    // if you're frustrated, can't reach the player easily, and don't have something to fight, go home
                    if (!actor.Acted && Frustration >= FrustrationLimit && !m.CanReach(Player, ignoreDoors: false))
                    {
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
                    if (!actor.Acted)
                    {
                        TargetPlayer(cr);
                    }                   
                }
            }
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnsSince += 1;
            if (Options.NoHumanAttacks)
            {
                return ge;
            }
            //if (te.Turn == 5)
            if (TurnsSince > 1500 && Game.World.Random.Next(100) == 0)
            {
                TurnsSince = 0;
                BanditAttack();
            }
            return ge;
        }

        public void BanditAttack(bool debugCloser = false)
        {
            Game.SplashPanel.Splash(new List<ColoredText> {
               "A gang of bandits has been spotted near the border of your domain.",
                "They must be coming to loot your supplies.  You should either hide behind sturdy doors, or kill them and take their ill-gotten loot for your own."
            });
            bool xwall = (Game.World.Random.Next(2)==0);
            bool zero = (Game.World.Random.Next(2) == 0);
            int x0, y0;
            if (xwall)
            {
                x0 = (zero) ? 1 : Game.World.Width - 2;
                
                y0 = Game.World.Random.Next(Game.World.Height - 2) + 1;
                if (debugCloser)
                {
                    x0 = (zero) ? 75 : 180;
                    y0 = Game.World.Random.Next(Game.World.Height - 75) + 75;
                }
            }
            else
            {
                y0 = (zero) ? 1 : Game.World.Height - 2;
                x0 = Game.World.Random.Next(Game.World.Width - 2) + 1;
                if (debugCloser)
                {
                    y0 = (zero) ? 75 : 180;
                    x0 = Game.World.Random.Next(Game.World.Width - 75) + 75;
                }
            }
            // for repeatable testing
            x0 = 12;
            y0 = 12;
            EntryTile = new Coord(x0, y0, Game.World.GetGroundLevel(x0, y0));
            for (int i = 0; i < PastBanditAttacks + 3; i++)
            {
                string creature = (i % 3 == 2) ? "WolfHound" : "HumanBandit";
                var bandit = Entity.Spawn<Creature>(creature);
                bandit.PlaceNear(x0, y0, 0, max: 5);
                Bandits.Add(bandit);
                TargetPlayer(bandit);
                // do they occasionally get placed one step underground?
                Debug.WriteLine($"{bandit.Describe()} placed at {bandit.X} {bandit.Y}");
                if (i==0)
                {
                    Item loot = Item.SpawnNewResource("TradeGoods", 1);
                    bandit.GetComponent<Inventory>().Item = loot;
                }
            }
            
            // this would be a good time to calculate all the paths, yes?

            PastBanditAttacks += 1;
        }
        
        public GameEvent OnBanditAttack(GameEvent ge)
        {
            AttackEvent ae = (AttackEvent)ge;
            Attacker att = ae.Attacker;
            Creature cr = (Creature) att.Entity.Unbox();
            if (Bandits.Contains(cr))
            {
                if (ae.Defender.Entity.Unbox().TypeName == "Door")
                {
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
