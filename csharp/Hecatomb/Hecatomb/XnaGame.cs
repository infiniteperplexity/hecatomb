using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class XnaGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont tileFont;
        SpriteFont textFont;

        public XnaGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileFont = this.Content.Load<SpriteFont>("NotoSans");
            textFont = this.Content.Load<SpriteFont>("PTMono");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            const int SIZE = 18;
            const int DIM = 25;
            for (int i=0; i<DIM; i++)
            {
            	for (int j=0; j<DIM; j++)
            	{
            		spriteBatch.DrawString(tileFont, ".", new Vector2(i*SIZE, j*SIZE), Color.Black);
            	}
            }
			spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

//
//        protected override void Update(GameTime gameTime)
//        {
//            KeyboardState state = Keyboard.GetState();
//            
//            // If they hit esc, exit
//            if (state.IsKeyDown(Keys.Escape))
//                Exit();
//
//            // Move our sprite based on arrow keys being pressed:
//            if (state.IsKeyDown(Keys.Right) & !previousState.IsKeyDown(
//                Keys.Right))
//                position.X += 10;
//            if (state.IsKeyDown(Keys.Left) & !previousState.IsKeyDown(
//                Keys.Left))
//                position.X -= 10;
//            if (state.IsKeyDown(Keys.Up))
//                position.Y -= 10;
//            if (state.IsKeyDown(Keys.Down))
//                position.Y += 10;
//
//            base.Update(gameTime);
//
//            previousState = state;
//        }



//public void Draw(
//    Texture2D texture,
//    Nullable<Vector2> position,
//    Nullable<Rectangle> destinationRectangle,
//    Nullable<Rectangle> sourceRectangle,
//    Nullable<Vector2> origin,
//    float rotation,
//    Nullable<Vector2> scale,
//    Nullable<Color> color,
//    SpriteEffects effects,
//    float layerDepth
//)


//<CharacterRegions>
//  <CharacterRegion><!-- Normal letters -->
//    <Start>&#32;</Start>
//    <End>&#126;</End>
//  </CharacterRegion>
//  <CharacterRegion><!-- Greek letters -->
//    <Start>&#913;</Start>
//    <End>&#969;</End>
//  </CharacterRegion>
//</CharacterRegions>