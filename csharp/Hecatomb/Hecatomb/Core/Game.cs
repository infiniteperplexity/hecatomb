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
using System.Threading;

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
        public static SplashPanel SplashPanel;
        public static ForegroundPanel ForegroundPanel;

        public static ControlContext LastControls;
        public static ControlContext Controls;
        public static ControlContext DefaultControls;
        public static ControlContext CameraControls;
        public static TimeHandler Time;
        public static DateTime LastDraw;
        public static String GameName;

        //		public static GameEventHandler Events;
        public static HashSet<Coord> Visible;
        GraphicsDeviceManager graphics;
        SpriteBatch sprites;

        public static string DefaultGameName;
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
            DefaultGameName = "GameWorld";
            GameName = DefaultGameName;
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
            Visible = new HashSet<Coord>();
            LastDraw = DateTime.Now;
            base.Initialize();
            if (!Options.NoStartupScreen)
            {
                ShowIntro();
            }
            else
            {
                StartupThread();
                //StartGame();
            }
        }

        protected void ShowIntro()
        {
            Controls = new StaticMenuControls("Welcome to Hecatomb", new List<(Keys, ColoredText, Action)>() {
                (Keys.N, "New game.", StartGame),
                (Keys.R, "Restore game.", RestoreGame),
                (Keys.Q, "Quit.", QuitGame)
            });
            Controls.ImageOverride = new ImageOverride(graphics, sprites);
        }

        public Queue<Action> UpdateActions = new Queue<Action>();

        public void StartupThread()
        {
            World = new World(256, 256, 64, seed: System.DateTime.Now.Millisecond);
            WorldBuilder builder = new DefaultBuilder();
            builder.Build(World);
            //ShowIntro();
            Creature p = null;
            while (p == null)
            {
                p = World.PlacePlayer();
            }
            TurnHandler.HandleVisibility();
            Time.Frozen = false;
            if (!Options.NoStartupScreen)
            {
                Controls.Reset();
                ForegroundPanel.Splash(new List<ColoredText>{
                    "{yellow}Welcome to Hecatomb!",
                    " ",
                    "You are a necromancer: A despised sorceror who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                    " ",
                    "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                    " ",
                    "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"\\\" to turn off the messages.",
                    " ",
                    "{orange}(Press Space Bar to continue.)"
                }, frozen: false);
            }
            else
            {
                Controls = new DefaultControls();
            }
        }
        public void StartGame()
        {
            Game.GameName = "GameWorld";
            TheFixer.Purge();
            Controls.Reset();
            ForegroundPanel.Splash(new List<ColoredText>{
                "{yellow}Welcome to Hecatomb!",
                " ",
                "You are a necromancer: A despised sorceror who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                " ",
                "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                " ",
                "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"\\\" to turn off the messages.",
                " ",
                "(Building world...please wait.)"
            }, frozen: true);
            //Controls.Set(new FrozenControls());
            MainPanel.Dirty = true;
            Thread thread = new Thread(StartupThread);
            thread.Start();
        }

        //not currently used
        public void RestartGame()
        {
            Game.GameName = "GameWorld";
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Starting new game..."
            }, frozen: true);
            Controls.Set(new FrozenControls());
        }

        public void RestoreGame()
        {
            TheFixer.Purge();
            Commands.RestoreGameCommand();
        }

        public void QuitGame()
        {
            Exit();
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
            SplashPanel = new SplashPanel(graphics, sprites);
            ForegroundPanel = new ForegroundPanel(graphics, sprites);
            // why don't I initialize main panel??
            MenuPanel.Initialize();
            StatusPanel.Initialize();
            SplashPanel.Initialize();
            ForegroundPanel.Initialize();
            int Size = MainPanel.Size;
            int Padding = MainPanel.Padding;
            graphics.PreferredBackBufferWidth = Padding + (2 + Camera.Width) * (Size + Padding) + MenuPanel.Width;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = Padding + (2 + Camera.Height) * (Size + Padding) + StatusPanel.Height;   // set this value to the desired height of your window
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
                if (World != null)
                {
                    foreach (Particle p in World.Particles.ToList())
                    {
                        p.Update();
                    }
                    Time.Update();
                }
            }
            Controls?.HandleInput();
            if (!Time.Frozen)
            {
                if (World != null)
                {
                    if (World.Turns.PlayerActed)
                    {
                        World.Turns.AfterPlayerActed();
                    }
                    World.Turns.Try();
                }
            }
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            /*TimeSpan sinceDraw = DateTime.Now.Subtract(LastDraw);
            if (sinceDraw > TimeSpan.FromMilliseconds(500))
            {
                MainPanel.Dirty = true;
                MenuPanel.Dirty = true;
                StatusPanel.Dirty = true;
            }*/
            ControlContext.Redrawn = true;
            sprites.Begin();
            if (Controls?.ImageOverride != null)
            {
                Controls.ImageOverride.Draw();
            }
            else if (!Time.Frozen /*|| sinceDraw > TimeSpan.FromMilliseconds(500)*/)
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
                    SplashPanel.Dirty = true;
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
            if (SplashPanel.Dirty && SplashPanel.Active)
            {
                SplashPanel.DrawContent();
                SplashPanel.Dirty = false;
            }
            if (ForegroundPanel.Dirty && ForegroundPanel.Active)
            {
                ForegroundPanel.DrawContent();
                ForegroundPanel.Dirty = false;
            }
            sprites.End();
            base.Draw(gameTime);
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            TheFixer.Dump();
            base.OnExiting(sender, args);
        }
    }
}