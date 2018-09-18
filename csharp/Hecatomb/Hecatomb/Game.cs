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

namespace Hecatomb
{
	class Game
	{
		public static World world;
		public static Player player;
		public static RLRootConsole display;
		public static void Main(string[] args)
		{
			world = new World();
			player = new Player();
			// this little f*cker totally messes with how I wanted to structure the program, but I'll live...for now...
			display = new RLRootConsole("terminal8x8.png", Constants.WIDTH, Constants.HEIGHT, 8, 8, 1.6f, "Hecatomb");
      		display.Update += OnRootConsoleUpdate;
      		display.Render += OnRootConsoleRender;
      		display.Run();
		}
		
		private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
		{
			int WIDTH = Constants.WIDTH;
			int HEIGHT = Constants.HEIGHT;
			int [,] grid = world.tiles;
			RLKeyPress keyPress = display.Keyboard.GetKeyPress();
			HandleCommand(keyPress);
			for (int i=0; i<WIDTH; i++) {
	    		for (int j=0; j<HEIGHT; j++) {
	    			if (player.x==i && player.y==j) {
						display.Print(i, j, player.sym.ToString(), RLColor.White);
	    			} else if (grid[i,j]==0) {
	    				display.Print(i, j, ".", RLColor.White);
	    			} else {
	    				display.Print(i, j, "#", RLColor.White);
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
			}
			return false;
		}
	}
}