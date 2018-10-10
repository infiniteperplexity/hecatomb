﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/9/2018
 * Time: 2:27 PM
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

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
			// can easily piggyback covers in here
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
				
				player = Player.EID,
				turns = Turns,
				entities = Entities.Spawned,
				explored = Explored,
				events = Events.StringifyListeners(),
				tiles = terrainFIDs
				
			};
			var json = JsonConvert.SerializeObject(jsonready, Formatting.Indented);
			System.IO.File.WriteAllText(@"..\GameWorld.json", json);
			return json;
		}
		
		public void Parse(string json)
			
		{
			JObject parsed = (JObject) JsonConvert.DeserializeObject(json);
			// *** Terrains and Covers ***
			int[,,] tiles = parsed["tiles"].ToObject<int[,,]>();
			for (int i=0; i<Constants.WIDTH; i++)
			{
				for (int j=0; j<Constants.HEIGHT; j++)
				{
					for (int k=0; k<Constants.DEPTH; k++)
					{
//						 piggyback covers in here too
						Tiles[i,j,k] = (Terrain) FlyWeight.FlyWeightTypes[typeof(Terrain)][tiles[i,j,k]];
					}
				}
			}
			// *** Entities ***
			
			foreach (int eid in Entities.Spawned.Keys.ToList())
			{
				Entities.Spawned[eid].Despawn();
			}
			// won't be needed, eventually
			Entities.Spawned.Clear();
			Creatures.Clear();
			Features.Clear();
			Items.Clear();
			Tasks.Clear();
			int pid = (int) parsed["player"];
			Entities.MaxEID = -1;
			foreach (var child in parsed["entities"].Values())
			{
				// will this handle Player?  It's such a terrible way I do that...
				string t = (string) child["ClassName"];
				Type T = Type.GetType("Hecatomb." + t);
				GameEntity ge = (GameEntity) child.ToObject(T);
				Entities.Spawned[ge.EID] = ge;
				Entities.MaxEID = Math.Max(Entities.MaxEID, ge.EID);
				ge.Spawned = true;
				if (ge is TypedEntity)
				{
					Coord c = child.ToObject<Coord>();
					TypedEntity te = (TypedEntity) ge;
					te.Place(c.x, c.y, c.z, fireEvent: false);
				}
			}
			// *** Player ***
			
			Player = (Player) Entities.Spawned[pid];
			Explored = parsed.GetValue("explored").ToObject<HashSet<Coord>>();
			Player.HandleVisibility();
			Game.GraphicsDirty = true;
			// *** Turns ***
			Turns = parsed.GetValue("turns").ToObject<TurnHandler>();
			// *** Event Listeners ***
			var	events = parsed.GetValue("events").ToObject<Dictionary<string,Dictionary<int, string>>>();
			foreach (string type in events.Keys)
			{
				var listeners = events[type];
				listeners.Clear();
				foreach (int eid in listeners.Keys)
				{
					Type T = typeof(Func<GameEvent, GameEvent>);
					Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(T, Entities.Spawned[eid], listeners[eid]);
				}
			}
			Debug.WriteLine("What hath we wrought?");
//			string j = System.IO.File.ReadAllText(@"..\GameWorld.json");
//			var jobj = JsonConvert.DeserializeObject(j);
			
//			GameWorld parsed = new GameWorld();
			//return this;
		}
	}
}