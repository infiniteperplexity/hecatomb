using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		public static Player Player;
		public static GameEventHandler Events;
		public static GameCommands Commands;
		public static GameColors Colors;
		public static GameCamera Camera;
		public static Random Random;
//		public static GameEventHandler Events;
		const int SIZE = 18;
		const int PADDING = 3;
		public static HashSet<Tuple<int, int, int>> Visible;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont tileFont;
        SpriteFont symbolFont;
        SpriteFont textFont;
        static KeyboardState kstate;
        static MouseState mstate;
        Texture2D bgTexture;
        
        
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
        	IsMouseVisible = true;
            EntityType.LoadEntities();
			Random = new Random();
			Colors = new GameColors();
			Events = new GameEventHandler();
			World = new GameWorld();
			Commands = new GameCommands();
			Player = new Player("Necromancer");
			Player.Place(
				Constants.WIDTH/2,
				Constants.HEIGHT/2,
				World.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			);
			Camera = new GameCamera();
			graphics.PreferredBackBufferWidth = PADDING+Camera.Width*(SIZE+PADDING);  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = PADDING+Camera.Height*(SIZE+PADDING);   // set this value to the desired height of your window
			graphics.ApplyChanges();
			bgTexture = new Texture2D(graphics.GraphicsDevice, SIZE+PADDING, SIZE+PADDING);
			Color[] bgdata = new Color[(SIZE+PADDING)*(SIZE+PADDING)];
			for(int i=0; i<bgdata.Length; ++i)
			{
				bgdata[i] = Color.White;
			}
			bgTexture.SetData(bgdata);
			Camera.Center(Player.x, Player.y, Player.z);
			Creature zombie = new Creature("Zombie");
			zombie.Place(
				Player.x+3,
				Player.y+3,
				World.GroundLevel(Player.x+3, Player.y+3)			
			);
			Player.Minions.Add(zombie);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        
        public SpriteFont GetFont(char c)
        {
        	if (tileFont.GetGlyphs().ContainsKey(c))
        	{
        		return tileFont;
        	}
        	else if (symbolFont.GetGlyphs().ContainsKey(c))
        	{
        		return symbolFont;
        	}
        	return tileFont;
        }
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileFont = this.Content.Load<SpriteFont>("NotoSans");
            symbolFont = this.Content.Load<SpriteFont>("NotoSansSymbol");
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
			bool acted = HandleInput(gameTime);
			if (acted) {
				IEnumerable<Creature> creatures = World.Creatures;
				Creature[] actors = creatures.ToArray();
				foreach (Creature m in Player.Minions)
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
			int xOffset;
			int yOffset;
			string sym;
			Color fg;
			Color bg;
			for (int i=0; i<Camera.Width; i++) {
	    		for (int j=0; j<Camera.Height; j++) {
					int x = i + Camera.XOffset;
					int y = j + Camera.YOffset;
					c = new Tuple<int, int, int>(x, y, Camera.z);
					Creature cr = Game.World.Creatures[x,y,Camera.z];
					TaskEntity task = Game.World.Tasks[x,y,Camera.z];
					tile = grid[x,y,Camera.z];
					if (cr!=null) {
						bg = Colors[tile.BG];
						fg = Colors[cr.FG];
						sym = cr.Symbol.ToString();
					} else if (!Visible.Contains(c)) {
						bg = Colors["black"];
						fg = Colors["SHADOWFG"];
						sym = tile.Symbol.ToString();
					} else {
						bg = Colors[tile.BG];
						fg = Colors[tile.FG];
						sym = tile.Symbol.ToString();
	    			}
					if (task!=null)
					{
						bg = Colors["orange"];
					}
					var measure = tileFont.MeasureString(sym);
					xOffset = 11-(int) measure.X/2;
					yOffset = -7;
					var vbg = new Vector2(PADDING+i*(SIZE+PADDING),PADDING+j*(SIZE+PADDING));
					var vfg = new Vector2(xOffset+PADDING+i*(SIZE+PADDING), yOffset+PADDING+j*(SIZE+PADDING));
					spriteBatch.Draw(bgTexture, vbg, bg);
					spriteBatch.DrawString(tileFont, sym, vfg, fg);
	    		}
			}
			spriteBatch.End();
            base.Draw(gameTime);
        }
        
        
        private static bool HandleInput(GameTime gameTime) {
        	var m = Mouse.GetState();
        	var k = Keyboard.GetState();
        	if (k.Equals(kstate) && m.Equals(mstate)) {
        		return false;
        	}
        	kstate = k;
        	mstate = m;
        	bool acted = false;
		    if (k.IsKeyDown(Keys.Up))
		    {
		    	acted = Commands.MoveNorthCommand();
			}
			else if (k.IsKeyDown(Keys.Down))
			{
				acted = Commands.MoveSouthCommand();
			}
			else if (k.IsKeyDown(Keys.Left))
			{
			 	acted = Commands.MoveWestCommand();
			}
			else if (k.IsKeyDown(Keys.Right))
			{
			   	acted = Commands.MoveEastCommand();
			}
			else if (k.IsKeyDown(Keys.OemPeriod))
			{
			    acted = Commands.MoveDownCommand();
			}
			else if (k.IsKeyDown(Keys.OemComma))
			{
			    acted =  Commands.MoveUpCommand();
			}
			else if (k.IsKeyDown(Keys.Space))
			{
			    acted = Commands.Wait();
			}
			if(m.LeftButton == ButtonState.Pressed)
			{
				Coord c = GetCellAt(m.X, m.Y);
				if (Game.World.Tasks[c.x, c.y, c.z]==null) 
				{
					TaskEntity task = new TaskEntity("DigTask");
					task.Place(c.x, c.y, c.z);
				}
			}
			
			return acted;
		}
        
        public static Coord GetCellAt(int x, int y)
        {
        	Coord c = new Coord(x/(SIZE+PADDING)+Camera.XOffset,y/(SIZE+PADDING)+Camera.YOffset,Camera.z);
        	return c;
        }
    }
}
//
//
//private object getProperty(object containingObject, string propertyName)
//{
//    return containingObject.GetType().InvokeMember(propertyName, BindingFlags.GetProperty, null, containingObject, null);
//}
// 
//private void setProperty(object containingObject, string propertyName, object newValue)
//{
//    containingObject.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, containingObject, new object[] { newValue });
//}