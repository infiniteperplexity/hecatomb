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

    public class Team : FlyWeight<Team>
    {
        // enemies of all other teams
        public bool Xenophobic;
        // enemies of all including own team
        public bool Berserk;
        // list of names of enemy teams
        List<string> Enemies;
        public Team(
            string type,
            string[] enemies = null,
            bool xenophobic = false,
            bool berserk = false
        ) : base(type)
        {
            TypeName = type;
            Enemies = (enemies == null) ? new List<string>() : enemies.ToList();
            Xenophobic = xenophobic;
            Berserk = berserk;
        }

        public bool IsFriendly(Creature c)
        {
            Actor a = c.GetComponent<Actor>();
            return IsFriendly(a);
        }
        public bool IsFriendly(Actor a)
        {
            Team t = a.Team;
            return IsFriendly(t);
        }

        public bool IsFriendly(Team t)
        {
            if (Berserk)
            {
                return false;
            }
            if (t == this)
            {
                return true;
            }
            return false;
        }

        public bool IsHostile(Creature c)
        {
            Actor a = c.GetComponent<Actor>();
            if (this==PlayerTeam && a.DeclaredEnemy)
            {
                return true;
            }
            return IsHostile(a);
        }
        public bool IsHostile(Actor a)
        {
            if (this == PlayerTeam && a.DeclaredEnemy)
            {
                return true;
            }
            Team t = a.Team;
            return IsHostile(t);
        }

        public bool IsHostile(Team t)
        {
            if (Berserk)
            {
                return true;
            }
            if (Xenophobic && t != this)
            {
                return true;
            }
            if (Enemies.Contains(t.TypeName) || t.Enemies.Contains(TypeName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // I'm not sure whether cacheing is needed here or not.
        public HashSet<Creature> GetEnemies()
        {
            TeamTracker tt = Game.World.GetState<TeamTracker>();
            HashSet<Creature> enemies = new HashSet<Creature>();
            // this is crap...it's not symmetrical and it ignores berserk
            foreach (string enemy in Enemies)
            {
                foreach (int eid in tt.Membership[enemy])
                {
                    enemies.Add((Creature)Entities[eid]);
                }
            }
            if (this==PlayerTeam)
            {
                foreach (Creature c in Creatures)
                {
                    Actor a = c.GetComponent<Actor>();
                    if (a.DeclaredEnemy)
                    {
                        enemies.Add(c);
                    }
                }
            }
            return enemies;
        }
        public void AddMember(Creature c)
        {
            TeamTracker tt = Game.World.GetState<TeamTracker>();
            tt.Membership[TypeName].Add(c.EID);
        }

        public void RemoveMember(Creature c)
        {
            TeamTracker tt = Game.World.GetState<TeamTracker>();
            tt.Membership[TypeName].Remove(c.EID);
        }

        public static Team PlayerTeam = new Team(type: "PlayerTeam", enemies: new[] { "HumanTeam" });
        public static Team NeutralAnimals = new Team(type: "NeutralAnimals");
        public static Team Berserkers = new Team(type: "Berserkers", enemies: new[] { "PlayerTeam" });
        public static Team HumanTeam = new Team(type: "HumanTeam", enemies: new[] { "PlayerTeam" });
        //		



        //		public List<int> GetMembers()
        //		{
        //			
        //		}
    }
}
