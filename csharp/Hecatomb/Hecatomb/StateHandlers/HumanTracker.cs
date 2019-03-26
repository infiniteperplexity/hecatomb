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

        public HumanTracker()
        {
            AddListener<TurnBeginEvent>(OnTurnBegin);
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
            for (int i = 0; i < 3; i++)
            {
                var bandit = Entity.Spawn<Creature>("HumanBandit");
                bandit.PlaceNear(x0, y0, 0, max: 5);
                // do they occasionally get placed one step underground?
                Debug.WriteLine($"bandit placed at {bandit.X} {bandit.Y}");
            }
            
            //var leader = Entity.Spawn<LeaderComponent>();
            //leader.AddToEntity(bandit);
            //what about "ChainPlace"?  How did that work in JS?
            // in JS you do that on the abstract type.  I think it would be better to have it be a static method on Creature, etc. that acts on a list of creatures.

            // figure out where to put it
            // we'll want a "bandit leader" component

            // Okay there are actually a bunch of things here...
            // Potentially, we would have bandit leader and bandit follower components...the leader trends toward the player
            // whereas the followers trend 'round the leader.  Now, if the leader dies, does another one take over?  Yeah...we can make that a part of the Behavior.


            PastBanditAttacks += 1;
        }
    }
}
