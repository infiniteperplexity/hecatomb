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
				entities = Entities,
				explored = Explored,
				events = Events.StringifyListeners(),
				tiles = terrainFIDs
				
			};
			var json = JsonConvert.SerializeObject(jsonready, Formatting.Indented);
			Parse(json);
			//System.IO.File.WriteAllText(@"..\GameWorld.json", json);
			return json;
		}
		
		public void Parse(string json)
			
		{
			JObject parsed = (JObject) JsonConvert.DeserializeObject(json);
			// *** Terrains and Covers ***
			int[,,] tiles = parsed.GetValue("tiles").ToObject<int[,,]>();
			for (int i=0; i<Constants.WIDTH; i++)
			{
				for (int j=0; j<Constants.HEIGHT; j++)
				{
					for (int k=0; k<Constants.DEPTH; k++)
					{
						// piggyback covers in here too
//						Tiles[i,j,k] = (Terrain) FlyWeight.FlyWeightTypes[typeof(Terrain)][tiles[i,j,k]];
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
			int pid = parsed.GetValue("player").Value<int>();
			foreach (var child in parsed.GetValue("entities").Children())
			{
				// will this handle Player?  It's such a terrible way I do that...
				string t = child.Value<string>("SomeKindaType");
				Type T = Type.GetType("Hecatomb." + t);
				GameEntity ge = (GameEntity) child.ToObject(T);
				Entities.Spawned[ge.EID] = ge;
				if (T==typeof(Creature) || ge.EID==pid)
				{
					Creature cr = (Creature) ge;
					Creatures[cr.x, cr.y, cr.z] = cr;
				}
				else if (T==typeof(Feature))
				{
					Feature fr = (Feature) ge;
					Features[fr.x, fr.y, fr.z] = fr;
				}
				else if (T==typeof(Item))
				{
					Item it = (Item) ge;
					Items[it.x, it.y, it.z] = it;
				}
				else if (T==typeof(TaskEntity))
				{
					TaskEntity te = (TaskEntity) ge;
					Tasks[te.x, te.y, te.z] = te;
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
