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

        void LoadHecatombContent()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            InterfaceState.Commands = new HecatombCommands();
            InterfaceState.Controls = new ControlContext();
            InterfaceState.Camera = new Camera(47, 33);
            InterfaceState.Colors = new Colors();
            InterfaceState.MainPanel = new MainPanel(GraphicsDevice, sprites!, Content, InterfaceState.Camera);             
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
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            base.Initialize();
            var world = new World(256, 256, 64);
            GameState.World = world;
            var ws = new BuildWorldStrategy();
            ws.Generate();
            InterfaceState.Controls.PlayerIsReady();
            
        }
        protected override void LoadContent()
        {
            sprites = new SpriteBatch(GraphicsDevice);
            LoadHecatombContent();
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            InterfaceState.HandleInput();
            InterfaceState.PrepareSprites();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
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