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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                Roll = OldGame.World.Random.Arbitrary(20, OwnSeed()) + 1,
                //Roll = Game.World.Random.Next(20)+1,
            };
            Defender defender = attack.Defender;

            if (Entity.TryComponent<Minion>() != null)
            {
                attack.AccuracyModifier += OldGame.World.GetState<ResearchHandler>().GetMinionAccuracy();
                attack.DamageModifier += OldGame.World.GetState<ResearchHandler>().GetMinionDamage();
            }
            if (defender.Entity.TryComponent<Minion>() != null)
            {
                attack.EvasionModifier += OldGame.World.GetState<ResearchHandler>().GetMinionEvasion();
                attack.ToughnessModifier += OldGame.World.GetState<ResearchHandler>().GetMinionToughness();
                attack.ArmorModifier += OldGame.World.GetState<ResearchHandler>().GetMinionArmor();
            }



            OldGame.World.Events.Publish(attack);
            int evade = defender.Evasion - defender.Wounds + attack.EvasionModifier;
            // at this point in the JS code, we aggro the defender in most cases
            //            Debug.WriteLine(
            //$@"
            //roll: {attack.Roll}
            //roll+accuracy: {attack.Roll+Accuracy}
            //11+evade: {11+evade}
            //"
            //            );
            if (attack.Roll + Accuracy + attack.AccuracyModifier >= 11 + evade)
            {
                //Debug.WriteLine("hit");
                // defender switches targets if the attacker is closer
                if (t is Creature && Entity.Unbox() is Creature)
                {
                    t.GetComponent<Actor>().Provoke((Creature)Entity);
                }
                defender.Defend(attack);
            }
            else
            {
                //Debug.WriteLine("Miss");
            }
            if (Entity.Unbox() is Creature)
            {
                Entity.GetComponent<Actor>().Spend();
            }
        }


        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            if (obj["Accuracy"] != null)
            {
                Accuracy = (int)obj["Accuracy"];
            }
            if (obj["Damage"] != null)
            {
                Damage = (int)obj["Damage"];
            }
        }
    }
}