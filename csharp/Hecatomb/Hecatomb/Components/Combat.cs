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
        public void Attack(TypedEntity t)
        {
            AttackEvent attack = new AttackEvent()
            {
                Attacker = this,
                Defender = t.TryComponent<Defender>(),
                Roll = Game.World.Random.Next(20),
                Modifiers = new Dictionary<string, int>()
            };
            Defender defender = attack.Defender;
            int evade = defender.Evasion - defender.Wounds;
            Game.World.Events.Publish(attack);
            // at this point in the JS code, we aggro the defender in most cases
            if (attack.Roll + Accuracy >= 11 + evade)
            {
                defender.Defend(attack);
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
            int damage = attacker.Damage - Armor - Toughness;
            Endure(damage);
        }
        // probably a damage event
        public void Endure(int damage)
        {
            if (damage >= 20)
            {
                // critical damage (die)
                Wounds = 8;
            }
            else if (damage >= 17)
            {
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
                if (Wounds < 2)
                {
                    Wounds = 2;
                }
                else if (Wounds < 7)
                {
                    Wounds += 1;
                }
            }
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
                    Entity.Destroy();
                }
            }
        }
    }
}