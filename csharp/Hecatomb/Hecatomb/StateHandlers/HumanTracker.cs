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
        public Coord? TargetTile;

        public virtual void Act(Creature c)
        {

        }


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
                    List<Feature> doors = Features.Where(f => (f.TypeName == "Door")).ToList();
                    foreach (Feature door in doors)
                    {
                        if (m.CanReach(door, useLast: false))
                        {
                            actor.Target = door;
                            break;
                        }
                    }
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
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (!Options.HumanAttacks)
            {
                return ge;
            }
            TurnBeginEvent te = (TurnBeginEvent)ge;
            if (te.Turn==10)
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
                Creature peasant = Entity.Spawn<Creature>("AngryPeasant");
                Debug.WriteLine("spawning a peasant at "+x + " "+y);
                peasant.Place(x, y, Game.World.GetGroundLevel(x, y));
            }
            return ge;
        }

        public void BanditAttack(bool debugCloser = false)
        {
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
            Bandits = new List<EntityField<Creature>>();
            TargetTile = new Coord(x0, y0, Game.World.GetGroundLevel(x0, y0));
            for (int i = 0; i < 3; i++)
            {
                var bandit = Entity.Spawn<Creature>("HumanBandit");
                bandit.PlaceNear(x0, y0, 0, max: 5);
                Bandits.Add(bandit);
                bandit.GetComponent<Actor>().EncounterTracker = this;
                TargetPlayer(bandit);
                // do they occasionally get placed one step underground?
                Debug.WriteLine($"bandit placed at {bandit.X} {bandit.Y}");
            }
            
            // this would be a good time to calculate all the paths, yes?

            PastBanditAttacks += 1;
        }

        public override void Act(Creature c)
        {
            Actor actor = c.GetComponent<Actor>();
            if (Frustration>=50)
            {
                actor.TargetTile = TargetTile;
                actor.Target = null;
                // problem...this will acquire targets via alert automatically
            }
            
            if (actor.Target==null)
            {
                TargetPlayer(c);
            }
        }
        
        public GameEvent OnBanditAttack(GameEvent ge)
        {
            AttackEvent ae = (AttackEvent)ge;
            if (Bandits.Contains((Creature)ae.Attacker.Entity.Unbox()))
            {
                if (ae.Defender.Entity.Name == "Door")
                {
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
    }
}
