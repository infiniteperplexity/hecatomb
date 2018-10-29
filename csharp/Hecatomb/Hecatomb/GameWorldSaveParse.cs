/*
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
			int[,,] terrainFIDs = new int[Width, Height, Depth];
			// can easily piggyback covers in here
			for (int i=0; i<Width; i++)
			{
				for (int j=0; j<Height; j++)
				{
					for (int k=0; k<Depth; k++)
					{
						terrainFIDs[i,j,k] = Tiles[i,j,k].FID;
					}
				}
			}
			var turns = new {
				Turn = Turns.Turn,
				Queue = Turns.QueueAsIDs(Turns.Queue),
				Deck = Turns.QueueAsIDs(Turns.Deck)
			};
			var jsonready = new
			{
				random = Random,
				player = Player.EID,
				turns = turns,
				entities = Entities.Spawned,
				explored = Explored,
				events = Events.StringifyListeners(),
				tiles = terrainFIDs
				
			};
			var json = JsonConvert.SerializeObject(jsonready, Formatting.Indented, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
			System.IO.File.WriteAllText(@"..\GameWorld.json", json);
			return json;
		}
		
		public void Parse(string json)
			
		{
			JObject parsed = (JObject) JsonConvert.DeserializeObject(json);
			// *** Random Seed ***
			Random = parsed["random"].ToObject<GameRandom>();
			Random.Initialize();
			// *** Terrains and Covers ***
			int[,,] tiles = parsed["tiles"].ToObject<int[,,]>();
			for (int i=0; i<tiles.GetLength(0); i++)
			{
				for (int j=0; j<tiles.GetLength(1); j++)
				{
					for (int k=0; k<tiles.GetLength(2); k++)
					{
//						 piggyback covers in here too
						Tiles[i,j,k] = (Terrain) FlyWeight.Enumerated[typeof(Terrain)][tiles[i,j,k]];
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
					EntityType.Types[te.TypeName].Standardize(te);
					te.Place(c.X, c.Y, c.Z, fireEvent: false);
				}
				else if (ge is Task)
				{
					Task task = (Task) ge;
					task.Standardize();
				}
			}
			// *** Player ***
			
			Player = (Player) Entities.Spawned[pid];
			Explored = parsed.GetValue("explored").ToObject<HashSet<Coord>>();
			Player.HandleVisibility();
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
			// *** Turns ***
			Turns = parsed.GetValue("turns").ToObject<TurnHandler>();
			Turns.Queue = Turns.QueueAsActors(parsed["turns"]["Queue"].ToObject<Queue<int>>());
			Turns.Deck = Turns.QueueAsActors(parsed["turns"]["Deck"].ToObject<Queue<int>>());
//			
			// *** Event Listeners ***
			var	events = parsed.GetValue("events").ToObject<Dictionary<string,Dictionary<int, string>>>();
			foreach (string type in events.Keys)
			{
				var listeners = events[type];
				listeners.Clear();
				foreach (int eid in listeners.Keys)
				{
					Type T = typeof(Func<GameEvent, GameEvent>);
					if (type=="GameEvent")
					{
						Events.GlobalListeners[eid] = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(T, Entities.Spawned[eid], listeners[eid]);
					}
					else
					{
						Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(T, Entities.Spawned[eid], listeners[eid]);
					}
				}
			}
		}
	}
}
