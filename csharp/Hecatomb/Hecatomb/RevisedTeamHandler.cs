﻿using System;
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
    public class RevisedTeamHandler : StateHandler
    {
        //public Dictionary<string, List<int>> Membership;
        //private Dictionary<int, Dictionary<int, bool>> hostilityMatrix;

        public Dictionary<string, Dictionary<string, bool>> HostilityMatrix;

        public RevisedTeamHandler() : base()
        {
            //Membership = new Dictionary<string, List<int>>();
            //foreach (Team t in Team.Enumerated)
            //{
            //    Membership[t.TypeName] = new List<int>();
            //}
        }

        public override void Activate()
        {
            base.Activate();
            HostilityMatrix = new Dictionary<string, Dictionary<string, bool>>();
            var m = HostilityMatrix;
            // "Friendly" is the player and minions
            m["Friendly"] = new Dictionary<string, bool>();
            m["Friendly"]["Friendly"] = false;
            m["Friendly"]["Neutral"] = false;
            m["Friendly"]["Berserk"] = true;
            m["Friendly"]["Hostile"] = true;
            m["Friendly"]["Good"] = true;
            m["Friendly"]["Evil"] = true;
            // "Neutral" is peaceful animals
            m["Neutral"] = new Dictionary<string, bool>();
            m["Neutral"]["Friendly"] = false;
            m["Neutral"]["Neutral"] = false;
            m["Neutral"]["Berserk"] = true;
            m["Neutral"]["Hostile"] = false;
            m["Neutral"]["Good"] = false;
            m["Neutral"]["Evil"] = false;
            // "Berserk" is renegade zombies and hungry predators
            m["Berserk"] = new Dictionary<string, bool>();
            m["Berserk"]["Friendly"] = true;
            m["Berserk"]["Neutral"] = true;
            m["Berserk"]["Berserk"] = false;
            m["Berserk"]["Hostile"] = true;
            m["Berserk"]["Good"] = true;
            m["Berserk"]["Evil"] = true;
            // "Hostile" is formerly peaceful animals that have been angered
            m["Hostile"] = new Dictionary<string, bool>();
            m["Hostile"]["Friendly"] = true;
            m["Hostile"]["Neutral"] = false;
            m["Hostile"]["Berserk"] = true;
            m["Hostile"]["Hostile"] = false;
            m["Hostile"]["Good"] = false;
            m["Hostile"]["Evil"] = false;
            // "Good" includes humans, elves, and dwarves
            m["Good"] = new Dictionary<string, bool>();
            m["Good"]["Friendly"] = true;
            m["Good"]["Neutral"] = false;
            m["Good"]["Berserk"] = true;
            m["Good"]["Hostile"] = false;
            m["Good"]["Good"] = false;
            m["Good"]["Evil"] = true;
            // "Evil" includes necromancers and goblins
            m["Evil"] = new Dictionary<string, bool>();
            m["Evil"]["Friendly"] = true;
            m["Evil"]["Neutral"] = false;
            m["Evil"]["Berserk"] = true;
            m["Evil"]["Hostile"] = false;
            m["Evil"]["Good"] = true;
            m["Evil"]["Evil"] = false;
            //Game.World.Events.Subscribe<DespawnEvent>(this, OnDespawn);  
        } 

        // this isn't currently even state-based
        public List<Creature> GetEnemies(Creature cr1)
        {
            Actor a1 = cr1.GetComponent<Actor>();
            return Creatures.Where((Creature cr)=>(HostilityMatrix[a1.TeamName][cr.GetComponent<Actor>().TeamName])).ToList();
        }
    }
}
