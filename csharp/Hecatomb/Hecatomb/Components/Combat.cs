/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/29/2018
 * Time: 10:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
    /// <summary>
    /// Description of Combat.
    /// </summary>
    public class Attacker : Component
    {
        public int Accuracy;
        public int Damage;

        public Attacker() : base()
        {
        }
        public void Attack(PositionedEntity t)
        {
            AttackEvent attack = new AttackEvent()
            {
                Attacker = this,
                Defender = t.TryComponent<Defender>(),
                Roll = Game.World.Random.Next(20)+1,
                Modifiers = new Dictionary<string, int>()
            };
            Defender defender = attack.Defender;
            int evade = defender.Evasion - defender.Wounds;
            Game.World.Events.Publish(attack);
            // at this point in the JS code, we aggro the defender in most cases
            Debug.WriteLine("rolled " + attack.Roll);
            Debug.WriteLine("evade " + evade);
            if (attack.Roll + Accuracy >= 11 + evade)
            {
                defender.Defend(attack);
                Debug.WriteLine("Hit");
            }
            else
            {
                Debug.WriteLine("Miss");
            }
            Entity.GetComponent<Actor>().Spend();
        }
    }

    public class Defender : Component
    {
        public int Wounds;
        public int Toughness;
        public int Armor;
        public int Evasion;

        public Defender() : base()
        {
        }
        public void Defend(AttackEvent attack)
        {
            Attacker attacker = attack.Attacker;
            int damage = attack.Roll + attacker.Damage - Armor - Toughness;
            Endure(damage, attack);
        }
        // probably a damage event
        public void Endure(int damage, AttackEvent attack)
        {
            Creature ca = (Creature)attack.Attacker.Entity;
            Creature cd = (Creature) Entity;
            Debug.WriteLine("Total damage is " + damage);
            if (damage >= 20)
            {
                // critical damage (die)
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{red}" + $"{ca.Describe()} deals critical damage to {cd.Describe()}" });
                Wounds = 8;
            }
            else if (damage >= 17)
            {
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{orange}" + $"{ca.Describe()} deals severe damage to {cd.Describe()}" });
                // severe damage
                if (Wounds < 6)
                {
                    Wounds = 6;
                }
                else
                {
                    Wounds = 8;
                }
            }
            else if (damage >= 14)
            {
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{orange}" + $"{ca.Describe()} deals moderate damage to {cd.Describe()}" });
                // moderate damage
                if (Wounds < 4)
                {
                    Wounds = 4;
                }
                else
                {
                    Wounds += 2;
                }
            }
            else if (damage >= 8)
            {
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{yellow}" + $"{ca.Describe()} deals mild damage to {cd.Describe()}" });
                if (Wounds < 2)
                {
                    Wounds = 2;
                }
                else if (Wounds < 7)
                {
                    Wounds += 1;
                }
            }
            else
            {
                Game.World.Events.Publish(new SensoryEvent()
                {
                    Sight = $"{ca.Describe()} hits {cd.Describe()} but deals no damage."
                });
            }
            Debug.Print("Total wounds for {0} are {1}", Entity.Describe(), Wounds);
            // now tally wounds
            if (Wounds >= 8)
            {
                if (Entity == Game.World.Player)
                {
                    Debug.WriteLine("The player should die.");
                }
                else
                {
                    Debug.WriteLine("This creature should die.");
                    Game.World.Events.Publish(new SensoryEvent() { Sight = $"{cd.Describe()} dies!" });
                    Entity.Destroy();
                }
            }
        }
    }
}