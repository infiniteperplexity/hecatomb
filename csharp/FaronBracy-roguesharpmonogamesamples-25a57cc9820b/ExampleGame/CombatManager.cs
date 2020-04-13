using System.Collections.Generic;
using System.Diagnostics;
using RogueSharp.DiceNotation;

namespace ExampleGame
{
   public class CombatManager
   {
      private readonly Player _player;
      private readonly List<AggressiveEnemy> _aggressiveEnemies;
 
      public CombatManager( Player player, List<AggressiveEnemy> aggressiveEnemies )
      {
         _player = player;
         _aggressiveEnemies = aggressiveEnemies;
      }

      public void Attack( Figure attacker, Figure defender )
      {
         if ( Dice.Roll( "d20" ) + attacker.AttackBonus >= defender.ArmorClass )
         {
            int damage = attacker.Damage.Roll().Value;
            defender.Health -= damage;
            Debug.WriteLine( "{0} hit {1} for {2} and he has {3} health remaining.", attacker.Name, defender.Name, damage, defender.Health );
            if ( defender.Health <= 0 )
            {
               if ( defender is AggressiveEnemy )
               {
                  var enemy = defender as AggressiveEnemy;
                  _aggressiveEnemies.Remove( enemy );
               }
               Debug.WriteLine( "{0} killed {1}", attacker.Name, defender.Name );
            }
         }
         else
         {
            Debug.WriteLine( "{0} missed {1}", attacker.Name, defender.Name );
         }
      }

      public Figure FigureAt( int x, int y )
      {
         if ( IsPlayerAt( x, y ) )
         {
            return _player;
         }
         return EnemyAt( x, y );       

      }

      public bool IsPlayerAt( int x, int y )
      {
         return ( _player.X == x && _player.Y == y );
      }

      public AggressiveEnemy EnemyAt( int x, int y )
      {
         foreach ( var enemy in _aggressiveEnemies )
         {
            if ( enemy.X == x && enemy.Y == y )
            {
               return enemy;
            }
         }
         return null;
      }

      public bool IsEnemyAt( int x, int y )
      {
         return EnemyAt( x, y ) != null;
      }
   }
}
