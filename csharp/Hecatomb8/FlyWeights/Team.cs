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
        public Team[] Enemies;
        public Team(string type = "", Team[]? enemies = null) : base(type)
        {
            Enemies = enemies ?? new Team[0];
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
            type: "Friendly",
            enemies: new Team[] { Berserk, Hostile, Good, Evil }
        );
        public static readonly Team Neutral = new Team(
            type: "Neutral",
            enemies: new Team[] { Berserk }
        );
        public static readonly Team Hostile = new Team(
            type: "Hostile",
            enemies: new Team[] { Berserk, Friendly }
        );
        public static readonly Team Good = new Team(
            type: "Good",
            enemies: new Team[] { Berserk, Friendly, Evil }
        );
        public static readonly Team Evil = new Team(
            type: "Evil",
            enemies: new Team[] { Berserk, Friendly, Good }
        );
        public static readonly Team Berserk = new Team(
            type: "Berserk",
            enemies: new Team[] { Berserk, Hostile, Good, Evil, Neutral }
        );
    }
}