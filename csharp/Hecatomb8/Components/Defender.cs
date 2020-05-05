using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
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
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            Attacker attacker = attack.Attacker;
            int damageRoll = GameState.World!.Random.Next(20) + 1 + attack.DamageModifier;
            int damage = damageRoll + attacker.Damage - Armor - attack.ArmorModifier - Toughness - attack.ToughnessModifier;
            Endure(damage, attack);
        }
        // probably a damage event
        public void Endure(int damage, AttackEvent attack)
        {
            if (Entity?.UnboxBriefly() is null || !Entity.UnboxBriefly()!.Placed)
            {
                return;
            }
            if (HecatombOptions.Invincible)
            {
                if (Entity.UnboxBriefly() is Creature)
                {
                    if (Entity.UnboxBriefly() == Player)
                    {
                        return;
                    }
                    //if (Entity.UnboxBriefly().GetComponent<Actor>().Team == Teams.Friendly)
                    //{
                    //    return;
                    //}
                }
            }
            ComposedEntity? ca = attack.Attacker.Entity?.UnboxBriefly();
            if (ca is null)
            {
                return;
            }
            TileEntity cd = (TileEntity)Entity.UnboxBriefly()!;
            var (x, y, z) = cd.GetValidCoordinate();
            if (damage >= 20)
            {
                Senses.Announce(x, y, z, sight: "{red}" + $"{ca.Describe(capitalized: true)} deals critical damage to {cd.Describe()}.");
                Wounds = 8;
            }
            else if (damage >= 17)
            {
                //Debug.WriteLine("severe damage (can kill)");
                //(new BloodEmitter() { LifeSpan = 100 }).Place(Entity.X, Entity.Y, Entity.Z);
                Senses.Announce(x, y, z, sight: "{orange}" + $"{ca.Describe(capitalized: true)} deals severe damage to {cd.Describe()}.");
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
                Senses.Announce(x, y, z, sight: "{orange}" + $"{ca.Describe(capitalized: true)} deals moderate damage to {cd.Describe()}.");
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
                Senses.Announce(x, y, z, sight: "{yellow}" + $"{ca.Describe(capitalized: true)} deals mild damage to {cd.Describe()}.");
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
                Senses.Announce(x, y, z, sight: $"{ca.Describe(capitalized: true)} hits {cd.Describe()} but deals no damage.");
            }
            //Debug.Print("Total wounds for {0} are {1}", Entity.Describe(), Wounds);
            // now tally wounds
            ResolveWounds();
        }

        public void ResolveWounds()
        {
            if (Entity?.UnboxBriefly() is null)
            {
                return;
            }
            var (x, y, z) = Entity.UnboxBriefly()!.GetValidCoordinate();
            if (Wounds >= 8)
            {
                if (Entity.UnboxBriefly() == Player)
                {
                    //Commands.PlayerDies();
                }
                else
                {
                    //(new BloodEmitter() { LifeSpan = 200 }).Place(Entity.X, Entity.Y, Entity.Z);
                    Senses.Announce(x, y, z, sight: "{red}" + $"{Entity.UnboxBriefly()?.Describe(capitalized: true)} dies!");
                    Entity.UnboxBriefly()?.Destroy();
                }
            }
        }
    }
}
