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
		public static TypedEntity player;
		public static RLRootConsole display;
		public static Colors myColors;
		public static void Main(string[] args)
		{
			EntityType Player = new EntityType("Player");
			Player.Components = new Type[] {typeof(Position)};
			myColors = new Colors();
			world = new World();
			player = new TypedEntity(Player);
			player.x = 12;
			player.y = 12;
			
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
			Terrain [,] grid = world.tiles;
			RLKeyPress keyPress = display.Keyboard.GetKeyPress();
			HandleCommand(keyPress);
			for (int i=0; i<WIDTH; i++) {
	    		for (int j=0; j<HEIGHT; j++) {
	    			if (player.x==i && player.y==j) {
						display.Print(i, j, player.Symbol.ToString(), myColors[player.FG]);
					} else {
						display.Print(i, j, grid[i,j].Symbol.ToString(), myColors[player.FG]);
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
		    		player.y = Math.Max(1, player.y-1 ?? 1);
		    		return true;
			    }
			    else if ( keyPress.Key == RLKey.Down )
			    {
			    	player.y = Math.Min(HEIGHT-2, player.y+1 ?? HEIGHT-2);
			    	return true;
			    }
			    else if ( keyPress.Key == RLKey.Left )
			    {
			    	player.x = Math.Max(1, player.x-1 ?? 1);
			    	return true;
			    }
			    else if ( keyPress.Key == RLKey.Right )
			    {
			    	player.x = Math.Min(WIDTH-2, player.x+1 ?? WIDTH-2);
			    	return true;
			    }
			}
			return false;
		}
	}
}