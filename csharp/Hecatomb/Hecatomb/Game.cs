using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace Hecatomb
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Game
    {
    	public static GameWorld World;
		public static TypedEntity Player;
		public static GameCommands Commands;
		public static Colors myColors;
		public static Camera camera;
		public static Random Random;
		public static HashSet<Tuple<int, int, int>> Visible;
		
		/// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
        	Go();
        }
		public static void Go()
		{
			EntityType.LoadEntities();
			Random = new Random();
			myColors = new Colors();
			World = new GameWorld();
			Commands = new GameCommands();
			Player = new TypedEntity("Player");
			Player.Place(
				Constants.WIDTH/2,
				Constants.HEIGHT/2,
				World.GroundLevel(Constants.WIDTH/2, Constants.HEIGHT/2)
			);
			camera = new Camera();
			camera.Center(Player.x, Player.y, Player.z);
			TypedEntity zombie = new TypedEntity("Zombie");
			zombie.Place(
				Player.x+3,
				Player.y+3,
				World.GroundLevel(Player.x+3, Player.y+3)			
			);
			using (var xna = new XnaGame())
                xna.Run();
		}
		
		

    }
#endif
}