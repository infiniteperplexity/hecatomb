/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/9/2018
 * Time: 2:27 PM
 */
using System;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public partial class GameWorld
	{
		public string Stringify()
		{
			int[,,] terrainFIDs = new int[Constants.WIDTH, Constants.HEIGHT, Constants.DEPTH];
			for (int i=0; i<Constants.WIDTH; i++)
			{
				for (int j=0; j<Constants.HEIGHT; j++)
				{
					for (int k=0; k<Constants.DEPTH; k++)
					{
						terrainFIDs[i,j,k] = Tiles[i,j,k].FID;
					}
				}
			}
			var jsonready = new
			{
				events = Events.StringifyListeners(),
				player = Player.EID,
				entities = Entities,
				tiles = terrainFIDs
				
			};
			var json = JsonConvert.SerializeObject(jsonready, Formatting.Indented);
			System.IO.File.WriteAllText(@"..\GameWorld.json", json);
			return json;
		}
		
		public static GameWorld Parse(string json)
		{
			string j = System.IO.File.ReadAllText(@"..\GameWorld.json");
//			var jobj = JsonConvert.DeserializeObject(j);
			GameWorld parsed = new GameWorld();
			return parsed;
		}
	}
}
