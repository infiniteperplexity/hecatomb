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
        public static HecatombOptions Options;
    	public static World World;
		public static HecatombCommmands Commands;
		public static Colors Colors;
		public static Camera Camera;
		public static ContentManager MyContentManager;
		public static Dictionary<string, object> Caches;
		public static MainGamePanel MainPanel;
		public static MenuGamePanel MenuPanel;
		public static StatusGamePanel StatusPanel;
		
		public static ControlContext LastControls;
		public static ControlContext Controls;
		public static ControlContext DefaultControls;
		public static ControlContext CameraControls;
		public static TimeHandler Time;
		
//		public static GameEventHandler Events;
		public static HashSet<Coord> Visible;
        GraphicsDeviceManager graphics;
        SpriteBatch sprites;

        public Texture2D startup;

        public static global::Hecatomb.Game game;
        
        
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
        /// 
        protected override void Initialize()
        {
            game.Window.Title = "Hecatomb";
        	IsMouseVisible = true;
            EntityType.LoadEntities();
            MyContentManager = Content;
            Options = new HecatombOptions();
            Colors = new Colors();
			Time = new TimeHandler();
            Time.Frozen = true;
            Commands = new HecatombCommmands();
			DefaultControls = new DefaultControls();
			CameraControls = new CameraControls();
			Camera = new Camera();
			base.Initialize();
            if (!Options.NoStartupScreen)
            {
                ShowIntro();
            }
            else
            {
                StartGame();
            }
        }
        
        protected void ShowIntro()
        {
            Controls = new StaticMenuControls("Welcome to Hecatomb", new List<(Keys, ColoredText, Action)>() {
                (Keys.S, "Start game.", StartGame)
            });
            Controls.ImageOverride = new ImageOverride(graphics, sprites);
        }
        protected void StartGame()
        {
            World = new World(256, 256, 64, seed: System.DateTime.Now.Millisecond);
            WorldBuilder builder = new DefaultBuilder();
            builder.Build(World);
            World.GetState<AchievementHandler>();
            World.GetState<HumanTracker>();
            Controls = DefaultControls;
            //ShowIntro();
            Creature p = null;
            while (p==null)
            {
                p = World.PlacePlayer();
            }
            var t = Hecatomb.Entity.Spawn<TutorialHandler>();
            t.Activate();
            TurnHandler.HandleVisibility();
            Time.Frozen = false;
            Controls.Reset();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        protected override void LoadContent()
        {
            sprites = new SpriteBatch(GraphicsDevice);
            MainPanel = new MainGamePanel(graphics, sprites);
            MenuPanel = new MenuGamePanel(graphics, sprites);
            StatusPanel = new StatusGamePanel(graphics, sprites);
            MenuPanel.Initialize();
            StatusPanel.Initialize();
            int Size = MainPanel.Size;
            int Padding = MainPanel.Padding;
            graphics.PreferredBackBufferWidth = Padding+(2+Camera.Width)*(Size+Padding)+MenuPanel.Width;  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = Padding+(2+Camera.Height)*(Size+Padding)+StatusPanel.Height;   // set this value to the desired height of your window
            //graphics.ToggleFullScreen();
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
            if (!Time.Frozen)
            {
                foreach (Particle p in World.Particles.ToList())
                {
                    p.Update();
                }
                Time.Update();
            }
            Controls?.HandleInput();
            if (!Time.Frozen)
            { 
                if (World.Turns.PlayerActed)
                {
                    World.Turns.AfterPlayerActed();
                }
                World.Turns.Try();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ControlContext.Redrawn = true;
        	sprites.Begin();
            if (Controls?.ImageOverride != null)
            {
                Controls.ImageOverride.Draw();
            }
            else if (!Time.Frozen)
            {
                if (MainPanel.Dirty)
                {
                    GraphicsDevice.Clear(Color.Black);

                    MainPanel.DrawContent();
                    MainPanel.Dirty = false;


                }
                else
                {
                    MainPanel.DrawDirty();
                }
                if (StatusPanel.Dirty)
                {
                    StatusPanel.DrawContent();
                    StatusPanel.Dirty = false;
                }
            }
            if (MenuPanel.Dirty)
            {
                MenuPanel.DrawContent();
                MenuPanel.Dirty = false;
            }    
        	sprites.End();
           	base.Draw(gameTime);
        }
    }
}