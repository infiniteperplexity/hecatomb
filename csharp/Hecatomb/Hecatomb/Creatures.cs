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
    public partial class Creature
    {
    }

    public class Necromancer : Creature
    {
        public Necromancer()
        {
            // might be better as a property
            ClassName = "Necromancer";
            Name = "necromancer";
            FG = "magenta";
            Symbol = '@';
            AddComponent(new Movement());
            AddComponent(new Senses());
            AddComponent<Actor>(new Actor()).Team = "Friendly";
            AddComponent(new SpellCaster());
            AddComponent(new Attacker());
            AddComponent(new Defender());
        }  
    }
}

/*
{
	"Types":
		[
			{
				"Type" : "Necromancer",
				"Name": "necromancer",
				"FG" : "magenta",
				"Symbol" : "@",
				"Components" : {
					"Actor" : {},
					"Senses": {},
					"Movement": {},
					"SpellCaster": {},
					"Attacker": {},
					"Defender": {}
				}
			},
			{
				"Type" : "Zombie",
				"Name": "zombie",
				"FG" : "lime green",
				"Symbol" : "z",
				"Components" : {
					"Actor": {},
					"Senses": {},
					"Movement": {},
					"Minion": {},
					"Inventory": {},
					"Attacker": {},
					"Defender": {}
				}
			},
			{
				"Type" : "PackRat",
				"Name" : "pack rat",
				"FG" : "brown",
				"Symbol" : "r",
				"Components" : {
					"Actor": {"TeamName" : "NeutralAnimals"},
					"Senses": {},
					"Movement": {},
					"Inventory": {},
					"Attacker": {},
					"Defender": {}
				}
			},
			{
				"Type" : "HungryGhoul",
				"Name" : "ghoul",
				"FG" : "red",
				"Symbol" : "z",
				"Components" : {
					"Actor": {"TeamName" : "Berserkers"},
					"Senses": {},
					"Movement": {},
					"Inventory": {},
					"Attacker": {},
					"Defender": {}
				}
			},
			{
				"Type" : "AngryPeasant",
				"Name" : "peasant",
				"FG" : "brown",
				"Symbol" : "@",
				"Components" : {
					"Actor": {
						"TeamName" : "HumanTeam",
						"Goals": ["HuntForPlayer"]
					},
					"Senses": {},
					"Movement": {},
					"Inventory": {},
					"Attacker": {},
					"Defender": {}
				}
			},
			{
				"Type" : "HumanBandit",
				"Name" : "human bandit",
				"FG" : "brown",
				"Symbol" : "@",
				"Components" : {
					"Actor": {
						"TeamName" : "HumanTeam",
						"Goals": ["HuntForPlayer"]
					},
					"Senses": {},
					"Movement": {},
					"Inventory": {},
					"Attacker": {},
					"Defender": {}
				}
			}
		]
}*/