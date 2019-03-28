using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

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
            Creature cd = (Creature)Entity;
            Debug.WriteLine("Total damage is " + damage);
            if (damage >= 20)
            {
                // critical damage (die)
                (new BloodEmitter() { LifeSpan = 200 }).Place(Entity.X, Entity.Y, Entity.Z);
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{red}" + $"{ca.Describe()} deals critical damage to {cd.Describe()}" });
                Wounds = 8;
            }
            else if (damage >= 17)
            {
                //(new BloodEmitter() { LifeSpan = 100 }).Place(Entity.X, Entity.Y, Entity.Z);
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
                //(new BloodEmitter()).Place(Entity.X, Entity.Y, Entity.Z);
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
                //(new BloodEmitter()).Place(Entity.X, Entity.Y, Entity.Z);
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
                    Entity.Entity.Destroy();
                }
            }
        }
    }
}
