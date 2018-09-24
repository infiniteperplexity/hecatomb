using Microsoft.Xna.Framework;
using RogueSharp;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ExampleGame
{
   public class Camera
   {
      public Camera()
      {
         Zoom = 1.0f;
      }

      // Centered Position of the Camera in pixels.
      public Vector2 Position { get; private set; }
      public float Zoom { get; private set; }
      public float Rotation { get; private set; }

      // Height and width of the viewport window which should be adjusted when the player resizes the game window.
      public int ViewportWidth { get; set; }
      public int ViewportHeight { get; set; }

      // Center of the Viewport does not account for scale
      public Vector2 ViewportCenter
      {
         get
         {
            return new Vector2( ViewportWidth * 0.5f, ViewportHeight * 0.5f );
         }
      }

      // create a matrix for the camera to offset everything we draw, the map and our objects. since the
      // camera coordinates are where the camera is, we offset everything by the negative of that to simulate
      // a camera moving. we also cast to integers to avoid filtering artifacts
      public Matrix TranslationMatrix
      {
         get
         {
            return Matrix.CreateTranslation( -(int) Position.X, -(int) Position.Y, 0 ) *
               Matrix.CreateRotationZ( Rotation ) *
               Matrix.CreateScale( new Vector3( Zoom, Zoom, 1 ) ) *
               Matrix.CreateTranslation( new Vector3( ViewportCenter, 0 ) );
         }
      }

      public void AdjustZoom( float amount )
      {
         Zoom += amount;
         if ( Zoom < 0.25f )
         {
            Zoom = 0.25f;
         }
      }

      public void MoveCamera( Vector2 cameraMovement, bool clampToMap = false )
      {
         Vector2 newPosition = Position + cameraMovement;

         if ( clampToMap )
         {
            Position = MapClampedPosition( newPosition );
         }
         else
         {
            Position = newPosition;
         }
      }

      public Rectangle ViewportWorldBoundry()
      {
         Vector2 viewPortCorner = ScreenToWorld( new Vector2( 0, 0 ) );
         Vector2 viewPortBottomCorner = ScreenToWorld( new Vector2( ViewportWidth, ViewportHeight ) );

         return new Rectangle( (int) viewPortCorner.X, (int) viewPortCorner.Y, (int) ( viewPortBottomCorner.X - viewPortCorner.X ),
                                                (int) ( viewPortBottomCorner.Y - viewPortCorner.Y ) );
      }

      // Center the camera on specific pixel coordinates
      public void CenterOn( Vector2 position )
      {
         Position = position;
      }

      // Center the camera on a specific cell in the map
      public void CenterOn( Cell cell )
      {
         Position = CenteredPosition( cell, true );
      }

      private Vector2 CenteredPosition( Cell cell, bool clampToMap = false )
      {
         var cameraPosition = new Vector2( cell.X * Global.SpriteWidth, cell.Y * Global.SpriteHeight );
         var cameraCenteredOnTilePosition = new Vector2( cameraPosition.X + Global.SpriteWidth / 2,
                                                         cameraPosition.Y + Global.SpriteHeight / 2 );
         if ( clampToMap )
         {
            return MapClampedPosition( cameraCenteredOnTilePosition );
         }

         return cameraCenteredOnTilePosition;
      }

      // Clamp the camera so it never leaves the visible area of the map.
      private Vector2 MapClampedPosition( Vector2 position )
      {
         var cameraMax = new Vector2( Global.MapWidth * Global.SpriteWidth - ( ViewportWidth / Zoom / 2 ),
            Global.MapHeight * Global.SpriteHeight - ( ViewportHeight / Zoom / 2 ) );

         return Vector2.Clamp( position, new Vector2( ViewportWidth / Zoom / 2, ViewportHeight / Zoom / 2 ), cameraMax );
      }

      public Vector2 WorldToScreen( Vector2 worldPosition )
      {
         return Vector2.Transform( worldPosition, TranslationMatrix );
      }

      public Vector2 ScreenToWorld( Vector2 screenPosition )
      {
         return Vector2.Transform( screenPosition, Matrix.Invert( TranslationMatrix ) );
      }

      // Move the camera's position based on input
      public void HandleInput( InputState inputState, PlayerIndex? controllingPlayer )
      {
         Vector2 cameraMovement = Vector2.Zero;

         if ( inputState.IsScrollLeft( controllingPlayer ) )
         {
            cameraMovement.X = -1;
         }
         else if ( inputState.IsScrollRight( controllingPlayer ) )
         {
            cameraMovement.X = 1;
         }
         if ( inputState.IsScrollUp( controllingPlayer ) )
         {
            cameraMovement.Y = -1;
         }
         else if ( inputState.IsScrollDown( controllingPlayer ) )
         {
            cameraMovement.Y = 1;
         }
         if ( inputState.IsZoomIn( controllingPlayer ) )
         {
            AdjustZoom( 0.25f );
         }
         else if ( inputState.IsZoomOut( controllingPlayer ) )
         {
            AdjustZoom( -0.25f );
         }

         // to match the thumbstick behavior, we need to normalize non-zero vectors in case the user
         // is pressing a diagonal direction.
         if ( cameraMovement != Vector2.Zero )
         {
            cameraMovement.Normalize();
         }

         // scale our movement to move 25 pixels per second
         cameraMovement *= 25f;

         // move the camera
         MoveCamera( cameraMovement, true );
      }
   }
}
