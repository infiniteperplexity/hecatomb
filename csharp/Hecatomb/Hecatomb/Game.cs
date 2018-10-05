using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
    	public static GameWorld World;
		public static GameCommands Commands;
		public static GameColors Colors;
		public static GameCamera Camera;
		public static Random Random;
		public static ContentManager MyContentManager;
		public static Dictionary<string, object> Caches;
		public static MainGamePanel MainPanel;
		public static MenuGamePanel MenuPanel;
		public static StatusGamePanel StatusPanel;
		
		public static ControlContext Controls;
		public static ControlContext DefaultControls;
		
//		public static GameEventHandler Events;
		public static HashSet<Tuple<int, int, int>> Visible;
        GraphicsDeviceManager graphics;
        SpriteBatch sprites;
        
        public static Hecatomb.Game game;
        
        
        [STAThread]
        static void Main()
        {
        	Go();
        }
		public static void Go()
		{
			game = new Game();
			game.Run();
//			using (game = new Game())
//                game.Run();
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
        	IsMouseVisible = true;
            EntityType.LoadEntities();
            MyContentManager = Content;
			Random = new Random();
			Colors = new GameColors();
			World = new GameWorld();
			World.Initialize();
			Commands = new GameCommands();
			DefaultControls = new DefaultControlContext();
			Controls = DefaultControls;
			Player p = new Player("Necromancer");
			World.Player = p;
			p.Place(
				Constants.WIDTH/2,
				Constants.HEIGHT/2,
				World.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			);
			Camera = new GameCamera();
			Camera.Center(p.x, p.y, p.z);
			Creature zombie = new Creature("Zombie");
			zombie.Place(
				p.x+3,
				p.y+3,
				World.GroundLevel(p.x+3, p.y+3)			
			);
			p.Minions.Add(zombie);
            base.Initialize();
        }
//        
//        private void SetCaches()
//        {
//        	var bgcache = new {
//        		vectors: new Vector2[WIDTH, HEIGHT]();
//        	}
//        	Cache["BackgroundTiles"] = 
//        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sprites = new SpriteBatch(GraphicsDevice);
            MainPanel = new MainGamePanel(graphics, sprites);
            MenuPanel = new MenuGamePanel(graphics, sprites);
            StatusPanel = new StatusGamePanel(graphics, sprites);
            int Size = MainPanel.Size;
            int Padding = MainPanel.Padding;
            graphics.PreferredBackBufferWidth = Padding+(2+Camera.Width)*(Size+Padding)+MenuPanel.Width;  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = Padding+(2+Camera.Height)*(Size+Padding)+StatusPanel.Height;   // set this value to the desired height of your window
			graphics.ApplyChanges();
    

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
			Controls.HandleInput();
			Player p = World.Player;
			if (p.Acted) {
				p.Acted = false;
				IEnumerable<Creature> creatures = World.Creatures;
				Creature[] actors = creatures.ToArray();
				foreach (Creature m in p.Minions)
				{
					Minion minion = m.GetComponent<Minion>();
					if (minion.Task==null)
					{
						foreach (TaskEntity t in World.Tasks)
						{
							if (t.GetComponent<Task>().Worker==null)
							{
								t.GetComponent<Task>().Worker = m;
								minion.Task = t;
								break;
							}
						}
					}
				}
				foreach (Creature cr in actors)
				{
					Actor actor = cr.TryComponent<Actor>();
					if (actor!=null)
					{
						actor.Act();
					}
				}
			} 
			
			Camera.Center(p.x, p.y, p.z);
			Visible = p.GetComponent<Senses>().GetFOV();
			foreach (var t in Visible)
			{
				World.Explored.Add(t);
			}
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            sprites.Begin();
			var grid = World.Tiles;
			Tuple<char, string, string> glyph;
			for (int i=0; i<Camera.Width; i++) {
	    		for (int j=0; j<Camera.Height; j++) {
					int x = i + Camera.XOffset;
					int y = j + Camera.YOffset;
					glyph = Tiles.GetGlyph(x, y, Camera.z);
					MainPanel.DrawGlyph(i, j, glyph.Item1, glyph.Item2, glyph.Item3);
	    		}
			}
			MenuPanel.DrawContent();
			StatusPanel.DrawContent();
			sprites.End();
            base.Draw(gameTime);
        }
    }
}