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
using System.Runtime.ExceptionServices;

namespace Hecatomb8
{
    public class HecatombGame : Microsoft.Xna.Framework.Game
    {
        public static Action? QuitHook;
        public static Action? Deferred;
        public static bool DrawnSinceDefer;
        public static TimeSpan LastUpdateTime;
        public static TimeSpan? NoUpdateUntil;

        void LoadHecatombContent()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            InterfaceState.Commands = new HecatombCommands();
            InterfaceState.Camera = new Camera(47, 33);
            InterfaceState.Colors = new Colors();
            InterfaceState.MainPanel = new MainPanel(GraphicsDevice, sprites!, Content, InterfaceState.Camera, 286, 20, 994, 700);   
            InterfaceState.InfoPanel = new InformationPanel(GraphicsDevice, sprites!, Content, 0, 0, 280, 700);
            InterfaceState.MenuPanel = new MenuPanel(GraphicsDevice, sprites!, Content, 286, 0, 1280, 20);
            InterfaceState.DefaultControls = new DefaultControls();
            InterfaceState.CameraControls = new CameraControls();
            InterfaceState.ForegroundPanel = new FullScreenPanel(GraphicsDevice, sprites!, Content, 0, 0, 1280, 720);
            InterfaceState.PopupPanel = new PopupPanel(GraphicsDevice, sprites!, Content, 500, 200, 16 * 31, 16 * 13);
            if (HecatombOptions.NoStartupScreen)
            {
                GameManager.StartGame();
            }
            else
            {
                GameManager.SetUpTitle();
            }
        }
        // *** Default MonoGame stuff ***
        GraphicsDeviceManager graphics;
        SpriteBatch? sprites;
        static void Main()
        {
            Go();
        }
        public static void Go()
        {

            var game = new HecatombGame();
            using (game = new HecatombGame())
            {
                game.Run();
            }
        }
        public HecatombGame()
        {
            QuitHook = Exit;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            sprites = new SpriteBatch(GraphicsDevice);
            LoadHecatombContent();
        }
        protected override void UnloadContent()
        {
        }

        public static void DeferUntilAfterDraw(Action action)
        {
            DrawnSinceDefer = false;
            if (Deferred != null)
            {
                throw new Exception("You can't defer two actions at once.");
            }
            Deferred = action;
        }

        public static void Sleep(int millis)
        {
            // this gets used correctly in the Splash method, and arguably I should move the rest of the logic to this method as well
            NoUpdateUntil = LastUpdateTime + new TimeSpan(0, 0, 0, 0, millis);
        }
        protected override void Update(GameTime gameTime)
        {
            LastUpdateTime = gameTime.TotalGameTime;
            if (NoUpdateUntil is null || gameTime.TotalGameTime >= NoUpdateUntil)
            {
                try
                {
                    if (GameState.World != null)
                    {
                        Time.Update();
                    }
                    InterfaceState.HandleInput();
                    InterfaceState.PreparePanels();
                }
                catch (Exception e)
                {
                    ExceptionHandling.Handle(e);
                }
            }
            try
            {
                if (Deferred != null && DrawnSinceDefer)
                {
                    Deferred!();
                    Deferred = null;
                }
            }
            catch(Exception e)
            {
                ExceptionHandling.Handle(e);
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            DrawnSinceDefer = true;
            graphics.GraphicsDevice.Clear(Color.Black);
            sprites!.Begin();
            InterfaceState.DrawInterfacePanels();
            sprites!.End();
            base.Draw(gameTime);
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
        }
    }
}