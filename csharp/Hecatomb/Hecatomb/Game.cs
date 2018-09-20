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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	class Game
	{
		public static World world;
		public static TypedEntity player;
		public static RLRootConsole display;
		public static Colors myColors;
		public static Camera camera;
		
		public static void Main(string[] args)
		{
			EntityType.LoadEntities();
			myColors = new Colors();
			world = new World();
			player = new TypedEntity("Player") {
				x = Constants.WIDTH/2,
				y = Constants.HEIGHT/2,
				z = world.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			};
			camera = new Camera();
			camera.Center(player.x, player.y, player.z);
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
			Terrain [,,] grid = world.tiles;
			RLKeyPress keyPress = display.Keyboard.GetKeyPress();
			HandleCommand(keyPress);
			camera.Center(player.x, player.y, player.z);
			for (int i=0; i<WIDTH; i++) {
	    		for (int j=0; j<HEIGHT; j++) {
					int x = i + camera.XOffset;
					int y = j + camera.YOffset;
	    			if (player.x==x && player.y==y && player.z==camera.z) {
						display.Print(i, j, player.Symbol.ToString(), myColors[player.FG]);
					} else {
						display.Print(i, j, grid[x,y,camera.z].Symbol.ToString(), myColors[player.FG]);
	    			}
	    		}
			}
		}
		
		private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
    	{
      		display.Draw();
    	}
		
		private static bool HandleCommand(RLKeyPress keyPress) {
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			int DEPTH = Constants.DEPTH;
			if ( keyPress != null )
		  	{
		    	if ( keyPress.Key == RLKey.Up )
		    	{
		    		player.y = Math.Max(1, player.y-1);
		    		return true;
			    }
			    else if ( keyPress.Key == RLKey.Down )
			    {
			    	player.y = Math.Min(HEIGHT-2, player.y+1);
			    	return true;
			    }
			    else if ( keyPress.Key == RLKey.Left )
			    {
			    	player.x = Math.Max(1, player.x-1);
			    	return true;
			    }
			    else if ( keyPress.Key == RLKey.Right )
			    {
			    	player.x = Math.Min(WIDTH-2, player.x+1);
			    	return true;
			    }
			    else if ( keyPress.Key == RLKey.Period )
			    {
			    	player.z = Math.Max(1, player.z-1);
			    }
			    else if ( keyPress.Key == RLKey.Comma )
			    {
			    	player.z = Math.Min(DEPTH-2, player.z+1);
			    }
			}
			return false;
		}
	}
}