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
;                Creature cr = (Creature)actor.Entity.Unbox();
                if (Bandits.Contains(cr))
                {
                    // if this is one of our bandits acting...
                    Debug.WriteLine("flag 1");
                    Debug.WriteLine($"Frustration level: {Frustration}");
                    Movement m = cr.GetComponent<Movement>();
                    // if you've been banging on the door a while, de-target the player if you can't easily reach the player
                    if (Frustration >= 50 && (!m.CanReach(Player, ignoreDoors: false) || Tiles.QuickDistance(Player, (ae.Entity as Creature)) > 20))
                    {
                        Debug.WriteLine("flag 2");
                        actor.Target = null;
                    }
                    // then take a look around for nearby hostiles
                    if (!actor.Acted)
                    {
                        Debug.WriteLine("flag 3");
                        actor.Alert();
                    }

                    if (!actor.Acted)
                    {
                        Debug.WriteLine("flag 4");
                        // if you are frustrated or you cannot reached the player walk back to the entry tile 
                        if (Frustration >= 50 || actor.Target == null) // !!! is this sufficient, or do we need to run CanReach?
                        {
                            var (x, y, z) = EntryTile;
                            // if you're right near where you entered the map, despawn
                            if (Tiles.QuickDistance(cr.X, cr.Y, cr.Z, x, y, z) <= 1)
                            {
                                Debug.WriteLine("despawning now");
                                actor.Acted = true;
                                actor.Spend();
                                cr.Despawn(); // this should be without dying, so you don't drop lot
                                return ge;
                            }
                            else
                            {
                                actor.WalkToward(x, y, z);
                            }
                        }
                    }
                    if (!actor.Acted)
                        actor.Wander();
                }
            }
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (!Options.HumanAttacks)
            {
                return ge;
            }
            TurnBeginEvent te = (TurnBeginEvent)ge;
            if (te.Turn==1000)
            {
                int side = Game.World.Random.Next(4);
                int x, y;
                if (side==0)
                {
                    x = 1;
                    y = Game.World.Random.Next(1, 254);
                }
                else if (side==1)
                {
                    x = 254;
                    y = Game.World.Random.Next(1, 254);
                }
                else if (side==2)
                {
                    y = 1;
                    x = Game.World.Random.Next(1, 254);
                }
                else
                {
                    y = 254;
                    x = Game.World.Random.Next(1, 254);
                }
                //Creature peasant = Entity.Spawn<Creature>("AngryPeasant");
                //Debug.WriteLine("spawning a peasant at "+x + " "+y);
                //peasant.Place(x, y, Game.World.GetGroundLevel(x, y));
                BanditAttack();
            }
            return ge;
        }

        public void BanditAttack(bool debugCloser = false)
        {
            Game.SplashPanel.Splash(new List<ColoredText> {
               "Your ravens report a gang of bandits near the border of your domain.",
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
            EntryTile = new Coord(x0, y0, Game.World.GetGroundLevel(x0, y0));
            for (int i = 0; i < 3; i++)
            {
                var bandit = Entity.Spawn<Creature>("HumanBandit");
                bandit.PlaceNear(x0, y0, 0, max: 5);
                Bandits.Add(bandit);
                TargetPlayer(bandit);
                // do they occasionally get placed one step underground?
                Debug.WriteLine($"bandit placed at {bandit.X} {bandit.Y}");
                if (i==0)
                {
                    Item loot = Item.SpawnNewResource("TradeGoods", 1);
                    bandit.GetComponent<Inventory>().Item = loot;
                }
            }
            
            // this would be a good time to calculate all the paths, yes?

            PastBanditAttacks += 1;
        }

        public override void Act(Creature c)
        {
            Actor actor = c.GetComponent<Actor>();
            // if you have already attacked a door a bunch of times, you stop ignoring doors when seeking targets
            if (Frustration>=50)
            {
                //...if you can reach the player without passing a door, and the player is within a certain distance, go after the player
                if (c.GetComponent<Movement>().CanReach(Player) && Tiles.QuickDistance(c.X, c.Y, c.Z, Player.X, Player.Y, Player.Z)<=20)
                {
                    TargetPlayer(c);
                    return;
                }
                // if there's something hostile nearby, alert to it as usual
                actor.Alert();
                // if there's nothing hostile and you can't easily reach the player, head back to where you entered the map
                if (!actor.Acted)
                {
                    var (x, y, z) = EntryTile;
                    // if you're right near where you entered the map, despawn
                    if (Tiles.QuickDistance(c.X, c.Y, c.Z, x, y, z) <= 1)
                    {
                        Debug.WriteLine("despawning now!");
                        c.Despawn(); // this should be without dying, so you don't drop lot
                    }
                    else
                    { 
                        actor.WalkToward(x, y, z);
                    }
                }
            }
            // if you got distracted on the way to the player, try to target the player again
            else if (actor.Target==null)
            {
                TargetPlayer(c);
            }
        }
        
        public GameEvent OnBanditAttack(GameEvent ge)
        {
            AttackEvent ae = (AttackEvent)ge;
            Attacker att = ae.Attacker;
            Creature cr = (Creature) att.Entity.Unbox();
            if (Bandits.Contains(cr))
            {
                Debug.WriteLine("flag X");
                if (ae.Defender.Entity.Unbox().TypeName == "Door")
                {
                    Debug.WriteLine($"Frustration level is now {Frustration}");
                    Frustration += 1;
                }
            }
            if (Frustration>=50)
            {
                Debug.WriteLine("Bandits tired of attacking, should go home now.");
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
