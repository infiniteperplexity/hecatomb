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

    class HumanTracker : StateHandler
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
            for (int i = 0; i < 3; i++)
            {
                var bandit = Entity.Spawn<Creature>("HumanBandit");
                bandit.PlaceNear(x0, y0, 0, max: 5);
                Bandits.Add(bandit);
                bandit.GetComponent<Actor>().EncounterTracker = this;
                // do they occasionally get placed one step underground?
                Debug.WriteLine($"bandit placed at {bandit.X} {bandit.Y}");
            }
            
            // this would be a good time to calculate all the paths, yes?

            PastBanditAttacks += 1;
        }

        public void Act(Creature c)
        {
            //this should attempt to replace the normal AI
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
