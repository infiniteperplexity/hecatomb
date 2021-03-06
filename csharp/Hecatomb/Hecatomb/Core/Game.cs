﻿using System;
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
using System.Runtime.ExceptionServices;

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
        public static MainPanel MainPanel;
        public static CommandsPanel MenuPanel;
        public static InformationPanel InfoPanel;
        public static SplashPanel SplashPanel;
        public static FullScreenPanel ForegroundPanel;

        public static ControlContext LastControls;
        public static ControlContext Controls;
        public static ControlContext DefaultControls;
        public static ControlContext CameraControls;
        public static ControlContext ReconstructControls;
        public static TimeHandler Time;
        public static DateTime LastDraw;
        public static String GameName;


        //		public static GameEventHandler Events;
        public static HashSet<Coord> Visible;
        public static GraphicsDeviceManager Graphics;
        public static SpriteBatch Sprites;

        public static string DefaultGameName;
        public Texture2D startup;

        public static global::Hecatomb.Game game;
        public static DateTime BuildDate;

        public static bool ReconstructMode;
        public static int FixedSeed = -1;


        [STAThread]
        static void Main()
        {
       
            BuildDate = BuildHandler.GetBuildDate();
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
            Graphics = new GraphicsDeviceManager(this);
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
            SetUpTitle();
        }

        public void SetUpTitle()
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
            World = null;
            Commands = new HecatombCommmands();
            DefaultControls = new DefaultControls();
            ControlContext.Initialize(DefaultControls);
            CameraControls = new CameraControls();
            ReconstructControls = new ReconstructControls();
            Camera = new Camera();
            Visible = new HashSet<Coord>();
            LastDraw = DateTime.Now;
            Graphics.ApplyChanges();
            if (Options.FullScreen)
            {
                Graphics.IsFullScreen = true;
            }
            base.Initialize();
            if (!Options.NoStartupScreen)
            {
                Debug.WriteLine("we got at least this far...");
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
            var commands = new List<(Keys, ColoredText, Action)>() {
                (Keys.N, "New game.", StartGame),
                (Keys.R, "Restore game.", RestoreGame),
                (Keys.Q, "Quit.", QuitGame)
            };
            if (Options.ReconstructGames)
            {
                commands.Add((Keys.C, "Reconstruct game from log.", ReconstructGame));
            }
            Controls = new StaticMenuControls("{yellow}Welcome to Hecatomb!", commands);
            MainPanel.IntroState = true;
        }

        public Queue<Action> UpdateActions = new Queue<Action>();

        public void StartupThread()
        {
            try
            {
                
                int seed = StatefulRandom.GetTimeSeed();
                if (FixedSeed > -1)
                {
                    seed = FixedSeed;
                }
                World = new World(256, 256, 64, seed: seed);
                WorldBuilder builder = new DefaultBuilder();
                builder.Build(World);
                //ShowIntro();
                Creature p = null;
                while (p == null)
                {
                    p = World.PlacePlayer();
                }
                ControlContext.HideCursor();
                TurnHandler.HandleVisibility();
                Time.Frozen = false;
                if (ReconstructMode)
                {
                    var tutorial = Game.World.GetState<TutorialHandler>();
                    if (tutorial.Visible)
                    {
                        Game.World.Events.Publish(new TutorialEvent() { Action = "HideTutorial" });
                    }
                    tutorial.Visible = false;
                    var logger = World.GetState<CommandLogger>();
                    foreach (GameCommand gc in CrashReportFile.LoggedCommands)
                    {
                        logger.CommandQueue.Enqueue(gc);
                    }
                    ControlContext.Reset();
                }
                else if (!Options.NoStartupScreen)
                {
                    ControlContext.Reset();
                    ForegroundPanel.Splash(new List<ColoredText>{
                        "{yellow}Welcome to Hecatomb!",
                        " ",
                        "You are a necromancer: A despised sorcerer who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                        " ",
                        "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                        " ",
                        "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"/\" to turn off the messages.",
                        " ",
                        "{orange}(Press Space Bar to continue.)"
                    }, frozen: false);
                }
                else
                {
                    ControlContext.Set(DefaultControls);
                }
                Game.World.Random.Poll();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public void StartGameWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", StartGame));
        }
        public void StartGame()
        {

            //string archiveFolder = Path.Combine(currentDirectory, "archive");
            //string[] files = Directory.GetFiles(archiveFolder, "*.zip");

            MainPanel.IntroState = false;
            Game.GameName = "GameWorld";
            TheFixer.Purge();
            ControlContext.Reset();
            ForegroundPanel.Splash(new List<ColoredText>{
                "{yellow}Welcome to Hecatomb!",
                " ",
                "You are a necromancer: A despised sorcerer who reanimates the dead to do your bidding.  Cast out from society, you flee to the wild hills to plot your revenge and purse the forbidden secrets of immortality.",
                " ",
                "{lime green}Cast spells, raise zombies from their graves, and command them to harvest resources and build you a fortress.  But beware: The forces of good will not long stand for your vile ways.",
                " ",
                "{cyan}Once the game begins, follow the in-game tutorial instructions on the right hand panel, or press \"/\" to turn off the messages.",
                " ",
                "(Building world...please wait.)"
            }, frozen: true);
            //Controls.Set(new FrozenControls());
            //InterfacePanel.DirtifyMainPanel();
            //InterfacePanel.DirtifyUsualPanels();
            Thread thread = new Thread(StartupThread);
            thread.Start();
        }

        //not currently used
        public void RestartGame()
        {
            MainPanel.IntroState = false;
            Game.GameName = "GameWorld";
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Starting new game..."
            }, frozen: true);
            ControlContext.Set(new FrozenControls());
        }


        public void RestoreGameWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", RestoreGame));
        }
        public void RestoreGame()
        {
            TheFixer.Purge();
            Commands.RestoreGameCommand();
        }

        public void ReconstructGame()
        {
            Commands.ReconstructGameCommand();
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
            Sprites = new SpriteBatch(GraphicsDevice);
            //MainPanel = new MainGamePanel(Graphics, Sprites);
            //MenuPanel = new MenuGamePanel(Graphics, Sprites);
            //StatusPanel = new StatusGamePanel(Graphics, Sprites);
            //SplashPanel = new SplashPanel(Graphics, Sprites);
            //ForegroundPanel = new ForegroundPanel(Graphics, Sprites);
            //Menu2Panel = new Menu2GamePanel(Graphics, Sprites);
            //why don't I initialize main panel??
            //MenuPanel.Initialize();
            //StatusPanel.Initialize();
            //SplashPanel.Initialize();
            //ForegroundPanel.Initialize();
            //Menu2Panel.Initialize();
            //int Size = MainPanel.Size;
            //int Padding = MainPanel.Padding;
            //graphics.PreferredBackBufferWidth = Padding + (2 + Camera.Width) * (Size + Padding) + MenuPanel.Width;  // set this value to the desired width of your window
            //graphics.PreferredBackBufferHeight = Padding + (2 + Camera.Height) * (Size + Padding) + StatusPanel.Height;   // set this value to the desired height of your window
            Debug.WriteLine($"original width was {Graphics.PreferredBackBufferWidth}");
            Debug.WriteLine($"original height was {Graphics.PreferredBackBufferHeight}");
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
            //graphics.ToggleFullScreen();
            Graphics.ApplyChanges();

            SplashPanel = new SplashPanel(35 + 276, 150, 16 * 31, 16 * 13);
            ForegroundPanel = new FullScreenPanel(0, 0, 1280, 720);
            MenuPanel = new CommandsPanel(286, 0, 1280, 20);
            InfoPanel = new InformationPanel(0, 0, 286, 700);
            MainPanel = new MainPanel(286, 20, 994, 700);
            InterfacePanel.AddPanel(MainPanel);
            InterfacePanel.AddPanel(MenuPanel);
            InterfacePanel.AddPanel(InfoPanel);
            InterfacePanel.AddPanel(SplashPanel);
            InterfacePanel.AddPanel(ForegroundPanel);

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
        /// 

        public static void HandleException(Exception e)
        {
            Debug.WriteLine(e.ToString());
            if (!Options.NoErrorLog)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                System.IO.Directory.CreateDirectory(path + @"\logs");
                var body = new List<string>();
                body.Add("Build Date:" + "\"" + Game.BuildDate.ToString() + "\"");
                body.Add(" ");
                body.Add(e.ToString());
                string json = "";
                if (Game.World != null)
                {
                    json = CommandLogger.DumpLog();
                    body[0] += (", Seed: " + "\"" + Game.World.Random.Seed + "\"");
                    body.Add(" ");
                    body.Add("Logged Commands:");
                    body.Add(json);
                }
                string filePath = path + @"\logs\" + "HecatombCrashReport" + timestamp + ".txt";
                System.IO.File.WriteAllLines(filePath, body);
                //string messageBody = "Oh no!  Hecatomb has crashed!  Please send this crash report to the supplied address.%0A%0A" + e.ToString();
                string messageBody = "Oh no!  Hecatomb has crashed!  Please send this crash report to the supplied address.  If possible, please attach the crash log file: " + filePath + "%0A%0A" + String.Join("%0A", body);
                try
                {
                    Process.Start(String.Format(
                        "mailto:{0}?subject={1}&body={2}",
                        "hecatomb.gamedev@gmail.com",
                        "Hecatomb crash report: " + timestamp,
                        messageBody
                    ));
                }
                catch (Exception e2)
                {
                    // this is allowed to fail silently
                }
                var replaced = (path + @"\logs\").Replace(@"\", "-").Replace(":", "~");
                try
                {
                    Process.Start("https://infiniteperplexity.github.io/hecatomb/crashReport.html?timestamp=" + timestamp + "&path=" + replaced);
                }
                catch (Exception e2)
                {
                    // this is allowed to fail silently
                }
            }
            // this method of handling preserves the stack trace
            var capturedException = ExceptionDispatchInfo.Capture(e);
            capturedException.Throw();
        }
        protected override void Update(GameTime gameTime)
        {
            try
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
                        World.Turns.Try();
                    }
                }
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            TimeSpan sinceDraw = DateTime.Now.Subtract(LastDraw);
            ControlContext.Redrawn = true;
            Sprites.Begin();
            if (MainPanel.IntroState)
            {
                MainPanel.Draw();
                InfoPanel.Draw();
                if (SplashPanel.Active)
                {
                    SplashPanel.Draw();
                }
                if (ForegroundPanel.Active)
                {
                    ForegroundPanel.Draw();
                }
            }
            else if (!Time.Frozen || sinceDraw > TimeSpan.FromMilliseconds(500))
            {
                if (!MainPanel.Dirty)
                {
                    MainPanel.DrawDirty();
                    // I totally don't remember why these lines of codes are necessary, but stuff gets weird without them
                    SplashPanel.Dirty = true;
                    ForegroundPanel.Dirty = true;
                }
                InterfacePanel.DrawPanels();
            }
            Sprites.End();
            base.Draw(gameTime);
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            TheFixer.Dump();
            base.OnExiting(sender, args);
        }

        public void BackToTitleWithConfirmation()
        {
            ControlContext.Set(new ConfirmationControls("Really quit the current game?", BackToTitle));
        }
        public void BackToTitle()
        {
            SplashPanel.Active = false;
            SetUpTitle();
        }
    }
}