/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 3:00 PM
 */
using System;
using System.IO;
using Newtonsoft.Json;

namespace Hecatomb
{
	public interface ISaveable
	{
		string Stringify();
	}
		
	public class GameJsonWriter : JsonWriter
	{
	
	    public override void WriteValue(ISaveable value)
	    {
	    	base.WriteValue(value.Stringify());
	    }
	}

	/// <summary>
	/// Description of GameSaver.
	/// </summary>
	static class GameSaver
	{
		public static void Save()
		{
			
			string tiles = JsonConvert.SerializeObject(Game.World.Tiles, new GameJsonWriter);
			System.IO.File.WriteAllText(@"..\WriteText.txt", json);
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
