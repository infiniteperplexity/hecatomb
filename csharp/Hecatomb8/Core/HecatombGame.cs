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
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class HecatombGame : Microsoft.Xna.Framework.Game
    {
        public static Action? QuitHook;
        public static Action? Deferred;
        public static bool DrawnSinceDefer;
        void LoadHecatombContent()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            InterfaceState.Commands = new HecatombCommands();
            InterfaceState.Camera = new Camera(47, 33);
            InterfaceState.Colors = new Colors();
            InterfaceState.MainPanel = new MainPanel(GraphicsDevice, sprites!, Content, InterfaceState.Camera, 286, 20);   
            InterfaceState.InfoPanel = new InformationPanel(GraphicsDevice, sprites!, Content, 0, 0);
            InterfaceState.DefaultControls = new DefaultControls();
            InterfaceState.CameraControls = new CameraControls();
            GameManager.SetUpTitle();
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
            Deferred = action;
        }
        protected override void Update(GameTime gameTime)
        {
            if (Deferred != null && DrawnSinceDefer)
            {
                Deferred!();
                Deferred = null;
            }
            if (GameState.World != null)
            {
                Time.Update();
            }
            InterfaceState.HandleInput();
            InterfaceState.PreparePanels();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            DrawnSinceDefer = true;
            sprites!.Begin();
            graphics.GraphicsDevice.Clear(Color.Black);
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