/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 3:00 PM
 */
using System;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Hecatomb
{

	/// <summary>
	/// Description of GameSaver.
	/// </summary>
	static class GameSaver
	{
		public static void Save()
		{
//			string tiles = JsonConvert.SerializeObject(Game.World.Tiles, new GameJsonWriter);
//			string test = JsonConvert.SerializeObject(Game.World.Player);
//			Debug.WriteLine(test);
//			Player p = JsonConvert.DeserializeObject<Player>(test);
//			Debug.WriteLine(p.Placed);
//			System.IO.File.WriteAllText(@"..\WriteText.txt", json);
		}
		static void Restore()
		{
			
		}
		
		static string StringifyWorld()
		{
			return "";
			
//				JsonConvert.SerializeObject(account, Formatting.Indented);
		}
		static GameWorld ParseWorld()
		{
			return new GameWorld();
		}
	}
}
