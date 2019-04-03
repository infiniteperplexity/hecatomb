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

        public void Attack(TypedEntity t)
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
            Debug.WriteLine(
$@"
roll: {attack.Roll}
roll+accuracy: {attack.Roll+Accuracy}
11+evade: {11+evade}
"
            );
            if (attack.Roll + Accuracy >= 11 + evade)
            {
                Debug.WriteLine("hit");
                defender.Defend(attack);

            }
            else
            {
                Debug.WriteLine("Miss");
            }
            Entity.GetComponent<Actor>().Spend();
        }
    }
}