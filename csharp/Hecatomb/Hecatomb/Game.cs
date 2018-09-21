/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 9:32 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using RLNET;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	class Game
	{
		public static GameWorld World;
		public static TypedEntity Player;
		public static RLRootConsole display;
		public static GameCommands Commands;
		public static Colors myColors;
		public static Camera camera;
		public static HashSet<Tuple<int, int, int>> Visible;
		
		public static void Main(string[] args)
		{
			EntityType.LoadEntities();
			myColors = new Colors();
			World = new GameWorld();
			Commands = new GameCommands();
			Player = new TypedEntity("Player") {
				x = Constants.WIDTH/2,
				y = Constants.HEIGHT/2,
				z = World.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			};
			camera = new Camera();
			camera.Center(Player.x, Player.y, Player.z);
			
			// this little f*cker totally messes with how I wanted to structure the program, but I'll live...for now...
			display = new RLRootConsole("terminal8x8.png", camera.Width, camera.Height, 8, 8, 1.6f, "Hecatomb");
      		display.Update += OnRootConsoleUpdate;
      		display.Render += OnRootConsoleRender;
      		
//      		
      		display.Run();
      		
		}
		
		private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
		{
			int WIDTH = camera.Width;
			int HEIGHT = camera.Height;
			Terrain [,,] grid = World.Tiles;
			RLKeyPress keyPress = display.Keyboard.GetKeyPress();
			HandleKeyPress(keyPress);
			camera.Center(Player.x, Player.y, Player.z);
			Visible = Player.GetComponent<Senses>().GetFOV();
			Tuple<int, int, int> c;
			Terrain tile;
			for (int i=0; i<WIDTH; i++) {
	    		for (int j=0; j<HEIGHT; j++) {
					int x = i + camera.XOffset;
					int y = j + camera.YOffset;
					c = new Tuple<int, int, int>(x, y, camera.z);
					if (Player.x==x && Player.y==y && Player.z==camera.z) {
						display.Print(i, j, Player.Symbol.ToString(), myColors[Player.FG]);
					} else if (!Visible.Contains(c)) {
						display.Print(i, j, " ", RLColor.Black);
					} else {
						tile = grid[x,y,camera.z];
						display.Print(i, j, tile.Symbol.ToString(), myColors[tile.FG]);
	    			}
	    		}
			}
		}
		
		private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
    	{
      		display.Draw();
    	}
		
		private static bool HandleKeyPress(RLKeyPress keyPress) {
			if ( keyPress != null )
		  	{
		    	if ( keyPress.Key == RLKey.Up )
		    	{
		    		return Commands.MoveNorthCommand();
			    }
			    else if ( keyPress.Key == RLKey.Down )
			    {
			    	return Commands.MoveSouthCommand();
			    }
			    else if ( keyPress.Key == RLKey.Left )
			    {
			    	return Commands.MoveWestCommand();
			    }
			    else if ( keyPress.Key == RLKey.Right )
			    {
			    	return Commands.MoveEastCommand();
			    }
			    else if ( keyPress.Key == RLKey.Period )
			    {
			    	return Commands.MoveDownCommand();
			    }
			    else if ( keyPress.Key == RLKey.Comma )
			    {
			    	return Commands.MoveUpCommand();
			    }
			}
			return false;
		}
	}
}