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
            int damageRoll = Game.World.Random.Next(20) + 1 + attack.DamageModifier;
            // in the JS version the damage roll is separate from the attack roll
            //int damage = attack.Roll + attacker.Damage - Armor - Toughness;
            int damage = damageRoll + attacker.Damage - Armor - attack.ArmorModifier - Toughness - attack.ToughnessModifier;
//            Debug.WriteLine(
//$@"
//damage roll: {damageRoll}
//total damage: {damage}
//current wounds: {Wounds}
//"
//            );
            Endure(damage, attack);
        }
        // probably a damage event
        public void Endure(int damage, AttackEvent attack)
        {
            if (Game.Options.Invincible)
            {
                if (Entity.Unbox() is Creature)
                {
                    if (Entity.Unbox() == Game.World.Player)
                    {
                        return;
                    }
                    if (Entity.GetComponent<Actor>().Team == Teams.Friendly)
                    {
                        return;
                    }
                }
            }
            Creature ca = (Creature)attack.Attacker.Entity;
            TileEntity cd = (TileEntity)Entity;
            if (damage >= 20)
            {
                //Debug.WriteLine("critical damage (one hit kill)");
                // critical damage (die)
                
                Game.World.Events.Publish(new SensoryEvent() { Sight = "{red}" + $"{ca.Describe()} deals critical damage to {cd.Describe()}" });
                Wounds = 8;
            }
            else if (damage >= 17)
            {
                //Debug.WriteLine("severe damage (can kill)");
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
                //Debug.WriteLine("moderate damage");
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
                //Debug.WriteLine("mild damage (cannot kill)");
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
            //Debug.Print("Total wounds for {0} are {1}", Entity.Describe(), Wounds);
            // now tally wounds
            ResolveWounds();
        }

        public void ResolveWounds()
        {
            if (Wounds >= 8)
            {
                if (Entity == Game.World.Player)
                {
                    Debug.WriteLine("The player should die.");
                    Game.SplashPanel.Splash(new List<ColoredText> {
                        "This is some placeholder text to make it obvious that the player would have died in a real game."
                    });
                }
                else
                {
                    Debug.WriteLine("This entity should die.");
                    (new BloodEmitter() { LifeSpan = 200 }).Place(Entity.X, Entity.Y, Entity.Z);
                    Game.World.Events.Publish(new SensoryEvent() { Sight = $"{Entity.Describe()} dies!" });
                    Entity.Unbox().Destroy();
                }
            }
        }
        public override void InterpretJSON(string json)
        {
            JObject obj = JObject.Parse(json);
            if (obj["Toughness"] != null)
            {
                Toughness = (int)obj["Toughness"];
            }
            if (obj["Evasion"] != null)
            {
                Evasion = (int)obj["Evasion"];
            }
        }
    }
}
