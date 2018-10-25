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
        /// 
        protected override void Initialize()
        {
        	
        	IsMouseVisible = true;
            EntityType.LoadEntities();
            MyContentManager = Content;
            Colors = new GameColors();
			Time = new TimeHandler();
			Commands = new GameCommands();
			DefaultControls = new DefaultControls();
			CameraControls = new CameraControls();
			Camera = new GameCamera();
			ShowIntro();
			StartGame();
			base.Initialize();
			StartGame();
        }
        
        protected void ShowIntro()
        {
        	World = new GameWorld(25, 25, 1);
        	WorldBuilder builder = new IntroBuilder();
			builder.Build(World);
        	Controls = DefaultControls;
//			Controls = new StaticMenuControls(
//				new List<Keys> {Keys.N},
//				new List<string> {"start game"},
//				new List<Action> {StartGame}
//			);
        	Player p = Game.World.Entities.Spawn<Player>("Necromancer");
			World.Player = p;
        	p.Initialize();
        	p.Place(12, 12, 0);
        	p.HandleVisibility();
        }
        protected  void StartGame()
        {
			World = new GameWorld(256, 256, 64, seed: System.DateTime.Now.Millisecond);
			WorldBuilder builder = new DefaultBuilder();
			builder.Build(World);
			Controls = DefaultControls;
			Player p = Game.World.Entities.Spawn<Player>("Necromancer");
			World.Player = p;
			p.Initialize();
			p.Place(
				World.Width/2,
				World.Height/2,
				World.GetGroundLevel(World.Width/2, World.Height/2)
			);
			Camera.Center(p.X, p.Y, p.Z);
			var t = Game.World.Entities.Spawn<TutorialTracker>();
			t.Activate();
			p.HandleVisibility();
			// proved it's possible to deserialize from method names
//			Debug.WriteLine("check this out...");
//			var f = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(typeof(Func<GameEvent, GameEvent>), World.Player, "OnPlace");
//			f(new PlaceEvent() {Entity = World.Player, X = 6, Y = 6, Z = 6});
            
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
//            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
//                Exit();

           	foreach (Particle p in MainPanel.Particles.ToList())
			{
				p.Update();
			}
           	Time.Update();
			Controls.HandleInput();
			if (World.Player.Acted)
           	{
           		World.Turns.PlayerActed();
           	}
			World.Turns.Try();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
        	sprites.Begin();
        	if (MainPanel.Dirty)
        	{
        		MainPanel.DrawContent();
        		MainPanel.Dirty = false;
        	}
        	else
        	{
        		MainPanel.DrawDirty();
        	}
        	if (MenuPanel.Dirty)
        	{
        		MenuPanel.DrawContent();
        		MenuPanel.Dirty = false;
        	}
        	if (StatusPanel.Dirty)
        	{
        		StatusPanel.DrawContent();
        		StatusPanel.Dirty = false;
        	}
        	sprites.End();
           	base.Draw(gameTime);
        }
    }
}