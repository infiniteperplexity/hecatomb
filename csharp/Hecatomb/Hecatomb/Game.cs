﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
    	public static GameWorld World;
		public static TypedEntity Player;
		public static GameCommands Commands;
		public static GameColors Colors;
		public static GameCamera Camera;
		public static Random Random;
		const int SIZE = 18;
		const int PADDING = 2;
		public static HashSet<Tuple<int, int, int>> Visible;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont tileFont;
        SpriteFont textFont;
        static KeyboardState kstate;
        
        
        [STAThread]
        static void Main()
        {
        	Go();
        }
		public static void Go()
		{
			using (var game = new Game())
                game.Run();
		}

        public Game()
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
            EntityType.LoadEntities();
			Random = new Random();
			Colors = new GameColors();
			World = new GameWorld();
			Commands = new GameCommands();
			Player = new TypedEntity("Player");
			Player.Place(
				Constants.WIDTH/2,
				Constants.HEIGHT/2,
				World.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			);
			Camera = new GameCamera();
			graphics.PreferredBackBufferWidth = PADDING+Camera.Width*(SIZE+PADDING);  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = PADDING+Camera.Height*(SIZE+PADDING);   // set this value to the desired height of your window
			graphics.ApplyChanges();
			Camera.Center(Player.x, Player.y, Player.z);
			TypedEntity zombie = new TypedEntity("Zombie");
			zombie.Place(
				Player.x+3,
				Player.y+3,
				World.GroundLevel(Player.x+3, Player.y+3)			
			);
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
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            int WIDTH = Camera.Width;
			int HEIGHT = Camera.Height;
			Terrain [,,] grid = World.Tiles;
			bool acted = HandleKeyPress();
			if (acted) {
				IEnumerable<TypedEntity> creatures = World.Creatures;
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
			var p = Player;
			Camera.Center(p.x, p.y, p.z);
			Visible = p.GetComponent<Senses>().GetFOV();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
			Tuple<int, int, int> c;
			Terrain tile;
			var grid = World.Tiles;
			for (int i=0; i<Camera.Width; i++) {
	    		for (int j=0; j<Camera.Height; j++) {
					int x = i + Camera.XOffset;
					int y = j + Camera.YOffset;
					c = new Tuple<int, int, int>(x, y, Camera.z);
					TypedEntity cr = Game.World.Creatures[x,y,Camera.z];
					var v = new Vector2(PADDING+i*(SIZE+PADDING),PADDING+j*(SIZE+PADDING));
					if (cr!=null) {
						spriteBatch.DrawString(tileFont, cr.Symbol.ToString(), v, Colors[cr.FG]);
					} else if (!Visible.Contains(c)) {
						spriteBatch.DrawString(tileFont, " ", v, Colors["black"]);
					} else {
						tile = grid[x,y,Camera.z];
						spriteBatch.DrawString(tileFont, tile.Symbol.ToString(), v, Colors[tile.FG]);
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