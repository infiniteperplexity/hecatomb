using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    /*
        Team:
            - Player (player and minions)
            - Berserk (attacks everything except Berserk) (renegade minions)
            - Hostile (attacks Player, Berserk.) (declared hostile neutrals)
            - Neutral (attacks Berserk.) (herbivores)
            - Good (attacks Player, Berserk, Evil.)
            - Evil (attacks Player, Berserk, Good.)
            How do we deal with carnivores?  I mean, we don't have any yet.
    */
    public class Team : FlyWeight<Team>
    {
        public List<Team> Enemies;
        public Team(string type = "") : base(type)
        {
            Enemies = new List<Team>();
        }

        public bool IsHostile(Team t)
        {
            return (Enemies.Contains(t));
        }
        public bool IsHostile(Creature c)
        {
            if (!c.Spawned)
            {
                return false;
            }
            else
            {
                return Enemies.Contains(c.GetComponent<Actor>().Team);
            }
        }


        public static readonly Team Friendly = new Team(
            type: "Friendly"
        );
        public static readonly Team Neutral = new Team(
            type: "Neutral"
        );
        public static readonly Team Hostile = new Team(
            type: "Hostile"
        );
        public static readonly Team Good = new Team(
            type: "Good"
        );
        public static readonly Team Evil = new Team(
            type: "Evil"
        );
        public static readonly Team Berserk = new Team(
            type: "Berserk"
        );

        static Team()
        {
            Friendly.Enemies.Add(Berserk);
            Friendly.Enemies.Add(Hostile);
            Friendly.Enemies.Add(Good);
            Friendly.Enemies.Add(Evil);
            Neutral.Enemies.Add(Berserk);
            Hostile.Enemies.Add(Berserk);
            Hostile.Enemies.Add(Friendly);
            Good.Enemies.Add(Berserk);
            Good.Enemies.Add(Friendly);
            Good.Enemies.Add(Evil);
            Evil.Enemies.Add(Berserk);
            Evil.Enemies.Add(Friendly);
            Evil.Enemies.Add(Good);
            Berserk.Enemies.Add(Berserk);
            Berserk.Enemies.Add(Friendly);
            Berserk.Enemies.Add(Hostile);
            Berserk.Enemies.Add(Good);
            Berserk.Enemies.Add(Evil);
            Berserk.Enemies.Add(Neutral);
        }
    }
}