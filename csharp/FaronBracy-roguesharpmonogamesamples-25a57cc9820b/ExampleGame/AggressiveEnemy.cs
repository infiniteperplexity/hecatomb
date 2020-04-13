using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;

namespace ExampleGame
{
   public class AggressiveEnemy : Figure
   {
      private readonly PathToPlayer _path;
      private readonly IMap _map;
      private bool _isAwareOfPlayer;
      
      public AggressiveEnemy( IMap map, PathToPlayer path )
      {
         _map = map;
         _path = path;
      }
      public void Draw( SpriteBatch spriteBatch )
      {
         spriteBatch.Draw( Sprite, new Vector2( X * Sprite.Width, Y * Sprite.Height ), null, null, null, 0.0f, Vector2.One, Color.White, SpriteEffects.None, LayerDepth.Figures );
         _path.Draw( spriteBatch );
      }
      public void Update()
      {
         if ( !_isAwareOfPlayer )
         {
            if ( _map.IsInFov( X, Y ) )
            {
               _isAwareOfPlayer = true;
            }
         }

         if ( _isAwareOfPlayer )
         {
            _path.CreateFrom( X, Y );
            if ( Global.CombatManager.IsPlayerAt( _path.FirstCell.X, _path.FirstCell.Y ) )
            {    
               Global.CombatManager.Attack( this, Global.CombatManager.FigureAt( _path.FirstCell.X, _path.FirstCell.Y ) );
            }
            else
            {
               X = _path.FirstCell.X;
               Y = _path.FirstCell.Y;
            }
         }
      }
   }
}