using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class XnaGame : Xna.Game
    {
        Xna.GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont tileFont;
        SpriteFont textFont;
        static KeyboardState kstate;

        public XnaGame()
        {
            graphics = new Xna.GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Game.camera.Width*18;  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = Game.camera.Height*18;   // set this value to the desired height of your window
			graphics.ApplyChanges();
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
            kstate = Keyboard.GetState();

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
        protected override void Update(Xna.GameTime gameTime)
        {
            if (GamePad.GetState(Xna.PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            int WIDTH = Game.camera.Width;
			int HEIGHT = Game.camera.Height;
			Terrain [,,] grid = Game.World.Tiles;
			bool acted = HandleKeyPress();
			if (acted) {
				IEnumerable<TypedEntity> creatures = Game.World.Creatures;
				TypedEntity[] actors = creatures.ToArray();
				foreach (TypedEntity cr in actors)
				{
					Actor actor = cr.TryComponent<Actor>();
					if (actor!=null)
					{
						actor.Act();
					}
				}
			} 
//			else {
//				this.SuppressDraw();
//			}
			var p = Game.Player;
			Game.camera.Center(p.x, p.y, p.z);
			Game.Visible = p.GetComponent<Senses>().GetFOV();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(Xna.GameTime gameTime)
        {
            GraphicsDevice.Clear(Xna.Color.Black);
            spriteBatch.Begin();
			Tuple<int, int, int> c;
			Terrain tile;
			const int SIZE = 18;
			var camera = Game.camera;
			var myColors = Game.myColors;
			var grid = Game.World.Tiles;
			for (int i=0; i<camera.Width; i++) {
	    		for (int j=0; j<camera.Height; j++) {
					int x = i + camera.XOffset;
					int y = j + camera.YOffset;
					c = new Tuple<int, int, int>(x, y, camera.z);
					TypedEntity cr = Game.World.Creatures[x,y,camera.z];
					var v = new Xna.Vector2(i*SIZE,j*SIZE);
					if (cr!=null) {
						spriteBatch.DrawString(tileFont, cr.Symbol.ToString(), v, Xna.Color.White);
					} else if (!Game.Visible.Contains(c)) {
						spriteBatch.DrawString(tileFont, " ", v, Xna.Color.Black);
					} else {
						tile = grid[x,y,camera.z];
						spriteBatch.DrawString(tileFont, tile.Symbol.ToString(), v, Xna.Color.White);
	    			}
	    		}
			}
			spriteBatch.End();
            base.Draw(gameTime);
        }
        
        
        private static bool HandleKeyPress() {
        	
        	var k = Keyboard.GetState();
        	if (k.Equals(kstate)) {
        		return false;
        	}
        	kstate = k;
        	var Commands = Game.Commands;
		    if (k.IsKeyDown(Keys.Up))
		    {
		    	return Commands.MoveNorthCommand();
			}
			else if (k.IsKeyDown(Keys.Down))
			{
				return Commands.MoveSouthCommand();
			}
			else if (k.IsKeyDown(Keys.Left))
			{
			 	return Commands.MoveWestCommand();
			}
			else if (k.IsKeyDown(Keys.Right))
			{
			   	return Commands.MoveEastCommand();
			}
			else if (k.IsKeyDown(Keys.OemPeriod))
			{
			    return Commands.MoveDownCommand();
			}
			else if (k.IsKeyDown(Keys.OemComma))
			{
			    return Commands.MoveUpCommand();
			}
			else if (k.IsKeyDown(Keys.Space))
			{
			    return Commands.Wait();
			}
			return false;
		}
    }
}