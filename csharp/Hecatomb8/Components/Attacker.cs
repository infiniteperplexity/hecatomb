using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Attacker : Component
    {
        public int Accuracy;
        public int Damage;

        public void Attack(ComposedEntity t)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            AttackEvent attack = new AttackEvent()
            {
                Attacker = this,
                Defender = t.GetComponent<Defender>(),
                Roll = GameState.World!.Random.Next(20)+1,
            };
            Defender defender = attack.Defender;

            if (Entity.UnboxBriefly()!.HasComponent<Minion>())
            {
                attack.AccuracyModifier += GetState<ResearchHandler>().GetMinionAccuracy();
                attack.DamageModifier += GetState<ResearchHandler>().GetMinionDamage();
            }
            if (t.HasComponent<Minion>())
            {
                attack.EvasionModifier += GetState<ResearchHandler>().GetMinionEvasion();
                attack.ToughnessModifier += GetState<ResearchHandler>().GetMinionToughness();
                attack.ArmorModifier += GetState<ResearchHandler>().GetMinionArmor();
            }
            Publish(attack);
            int evade = defender.Evasion - defender.Wounds + attack.EvasionModifier;
            if (attack.Roll + Accuracy + attack.AccuracyModifier >= 11 + evade)
            {
                // defender switches targets if the attacker is closer
                if (t is Creature && Entity.UnboxBriefly() is Creature)
                {
                    //t.GetComponent<Actor>().Provoke((Creature)Entity);
                }
                defender.Defend(attack);
            }
            else
            {
                //Debug.WriteLine("Miss");
            }
            if (Entity.UnboxBriefly() is Creature)
            {
                Entity.UnboxBriefly()!.GetComponent<Actor>().Spend();
            }
        }
    }
}