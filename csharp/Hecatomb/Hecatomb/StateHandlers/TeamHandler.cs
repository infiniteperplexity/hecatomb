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

    public enum Teams
    {
        Friendly = 0,
        Neutral = 1,
        Berserk = 2,
        Hostile = 3,
        Good = 4,
        Evil = 5
    }
    /*
        Teams:
            - Player (player and minions)
            - Berserk (attacks everything except Berserk) (renegade minions)
            - Hostile (attacks Player, Berserk.) (declared hostile neutrals)
            - Neutral (attacks Berserk.) (herbivores)
            - Good (attacks Player, Berserk, Evil.)
            - Evil (attacks Player, Berserk, Good.)
            How do we deal with carnivores?  I mean, we don't have any yet.
    */
    public class TeamHandler : StateHandler
    {

        [JsonIgnore]
        //public Dictionary<string, Dictionary<string, bool>> HostilityMatrix;
        public Dictionary<(Teams, Teams), bool> HostilityMatrix;

        public TeamHandler() : base()
        {
            HostilityMatrix = new Dictionary<(Teams, Teams), bool>();
            var m = HostilityMatrix;
            // "Friendly" is the player and minions
            m[(Teams.Friendly, Teams.Friendly)] = false;
            m[(Teams.Friendly, Teams.Neutral)] = false;
            m[(Teams.Friendly, Teams.Berserk)] = true;
            m[(Teams.Friendly, Teams.Hostile)] = true;
            m[(Teams.Friendly, Teams.Good)] = true;
            m[(Teams.Friendly, Teams.Evil)] = true;
            // "Neutral" is peaceful animals
            m[(Teams.Neutral, Teams.Friendly)] = false;
            m[(Teams.Neutral, Teams.Neutral)] = false;
            m[(Teams.Neutral, Teams.Berserk)] = true;
            m[(Teams.Neutral, Teams.Hostile)] = false;
            m[(Teams.Neutral, Teams.Good)] = false;
            m[(Teams.Neutral, Teams.Evil)] = false;
            // "Berserk" is renegade zombies and hungry predators
            m[(Teams.Berserk, Teams.Friendly)] = true;
            m[(Teams.Berserk, Teams.Neutral)] = true;
            m[(Teams.Berserk, Teams.Berserk)] = false;
            m[(Teams.Berserk, Teams.Hostile)] = true;
            m[(Teams.Berserk, Teams.Good)] = true;
            m[(Teams.Berserk, Teams.Evil)] = true;
            // "Hostile" is formerly peaceful animals that have been angered
            m[(Teams.Hostile, Teams.Friendly)] = true;
            m[(Teams.Hostile, Teams.Neutral)] = false;
            m[(Teams.Hostile, Teams.Berserk)] = true;
            m[(Teams.Hostile, Teams.Hostile)] = false;
            m[(Teams.Hostile, Teams.Good)] = false;
            m[(Teams.Hostile, Teams.Evil)] = false;
            // "Good" includes humans, elves, and dwarves
            m[(Teams.Good, Teams.Friendly)] = true;
            m[(Teams.Good, Teams.Neutral)] = false;
            m[(Teams.Good, Teams.Berserk)] = true;
            m[(Teams.Good, Teams.Hostile)] = false;
            m[(Teams.Good, Teams.Good)] = false;
            m[(Teams.Good, Teams.Evil)] = true;
            // "Evil" includes necromancers and goblins
            m[(Teams.Evil, Teams.Friendly)] = true;
            m[(Teams.Evil, Teams.Neutral)] = false;
            m[(Teams.Evil, Teams.Berserk)] = true;
            m[(Teams.Evil, Teams.Hostile)] = false;
            m[(Teams.Evil, Teams.Good)] = true;
            m[(Teams.Evil, Teams.Evil)] = false;
            //Game.World.Events.Subscribe<DespawnEvent>(this, OnDespawn);  
        } 
        
        public bool CheckHostile(Teams t1, Teams t2)
        {
            return HostilityMatrix[(t1, t2)];
        }
        public bool CheckHostile(Teams t1, Creature cr2)
        {
            return HostilityMatrix[(t1, cr2.GetComponent<Actor>().Team)];
        }
        public bool CheckHostile(Creature cr1, Teams t2)
        {
            return HostilityMatrix[(cr1.GetComponent<Actor>().Team, t2)];
        }


        public Creature GetClosestEnemy(Creature cr, int minDist = 12)
        {
            Actor actor = cr.GetComponent<Actor>();
            Movement move = cr.GetComponent<Movement>();
            Actor a;
            double dist = minDist;
            double d;
            Creature enemy = null;
            foreach (Creature c in Creatures)
            {
                a = c.GetCachedActor();
                //a = c.GetComponent<Actor>();
                if (HostilityMatrix[(actor.Team, a.Team)])
                {
                    d = Tiles.QuickDistance(cr.X, cr.Y, cr.Z, c.X, c.Y, c.Z);
                    if (d < dist && d < minDist)
                    {
                        if (move.CanReach(c))
                        {
                            enemy = c;
                            dist = d;
                        }
                    }
                }
            }
            //foreach (Entity e in Entities.Values)
            //{
            //    if (e is Actor)
            //    {
            //        a = (Actor)e;
            //        // seriously?  Unbox() is what takes so long?
            //        //c = (Creature)a.Entity.Unbox();
            //        c = (Creature)a.Entity;
            //        if (HostilityMatrix[actor.Team][a.Team])
            //        {
            //            d = Tiles.QuickDistance(cr.X, cr.Y, cr.Z, c.X, c.Y, c.Z);
            //            if (d < dist && d < minDist)
            //            {
            //                enemy = c;
            //                dist = d;
            //            }
            //        }
            //    }
            //}
            return enemy;
        }
        // this isn't currently even state-based
        public List<Creature> GetEnemies(Creature cr1)
        {
            Actor a1 = cr1.GetComponent<Actor>();
            List<Entity> actors = Entities.Values.Where((Entity e) => (e is Actor && !(e as Actor).Asleep)).ToList();
            List<Creature> creatures = actors.Select((Entity e) => ((e as Actor).Entity.Unbox() as Creature)).ToList();
            return creatures.Where((Creature cr)=>HostilityMatrix[(a1.Team, cr.GetComponent<Actor>().Team)]).ToList();
        }
    }
}
