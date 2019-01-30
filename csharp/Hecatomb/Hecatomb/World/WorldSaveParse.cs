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
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{

	public partial class World
	{
		public string Stringify()
		{
			int[,,] terrainFIDs = new int[Width, Height, Depth];
            int[,,] coverFIDs = new int[Width, Height, Depth];
			for (int i=0; i<Width; i++)
			{
				for (int j=0; j<Height; j++)
				{
					for (int k=0; k<Depth; k++)
					{
						terrainFIDs[i,j,k] = Terrains[i,j,k].FID;
                        coverFIDs[i, j, k] = Covers[i, j, k].FID;
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
                entities = Entities,
                explored = Explored,
                events = Events.StringifyListeners(),
                tiles = terrainFIDs,
                covers = coverFIDs
				
			};
            //foreach (GameEntity ge in Entities.Spawned.Values)
            //{
            //    Debug.Print("Trying to convert a {0}", ge.GetType());
            //    var j = JsonConvert.SerializeObject(ge, Formatting.Indented, new JsonSerializerSettings
            //    { NullValueHandling = NullValueHandling.Ignore });
            //}
            var json = JsonConvert.SerializeObject(jsonready, Formatting.Indented, new JsonSerializerSettings
                { NullValueHandling = NullValueHandling.Ignore});
            System.IO.File.WriteAllText(@"..\GameWorld.json", json);
			return json;
		}
		
		public void Parse(string json)
			
		{
			JObject parsed = (JObject) JsonConvert.DeserializeObject(json);
			// *** Random Seed ***
			Random = parsed["random"].ToObject<StatefulRandom>();
			Random.Initialize();
			// *** Terrains and Covers ***
			int[,,] tiles = parsed["tiles"].ToObject<int[,,]>();
            int[,,] covers = parsed["covers"].ToObject<int[,,]>();
            for (int i=0; i<tiles.GetLength(0); i++)
			{
				for (int j=0; j<tiles.GetLength(1); j++)
				{
					for (int k=0; k<tiles.GetLength(2); k++)
					{
//						 piggyback covers in here too
						Terrains[i,j,k] = Terrain.Enumerated[tiles[i,j,k]];
                        Covers[i, j, k] = Cover.Enumerated[covers[i, j, k]];
                    }
				}
			}
			// *** Entities ***
			
			foreach (int eid in Entities.Keys.ToList())
			{
                // this process can trigger recursive despawning
                if (Entities.ContainsKey(eid))
                {
                    Entities[eid].Despawn();
                }
			}
            // may want to double check that everything has been cleared?
			int pid = (int) parsed["player"];
			Entity.MaxEID = -1;
            Dictionary<int, Coord> Placements = new Dictionary<int, Coord>();
			foreach (var child in parsed["entities"].Values())
			{
                // will this handle Player?  It's such a terrible way I do that...
				string t = (string) child["ClassName"];
				Type T = Type.GetType("Hecatomb." + t);
				Entity ge = (Entity) child.ToObject(T);
                if (ge is TileEntity)
                {
                    Coord c = (Coord) child.ToObject(typeof(Coord));
                    Placements[ge.EID] = c;
                }
				Entities[ge.EID] = ge;
				Entity.MaxEID = Math.Max(Entity.MaxEID, ge.EID);
				ge.Spawned = true;
			}
            // okay, this gets weird...so...
            foreach (int eid in Placements.Keys.ToList())
            {
                Entity e = Entities[eid];
                if (e is TileEntity)
                {
                    TileEntity te = (TileEntity)e;
                    var (x, y, z) = Placements[eid];
                    if (te is TypedEntity)
                    {
                        TypedEntity tye = (TypedEntity)te;
                        EntityType.Types[tye.TypeName].Standardize(tye);
                    }
                    // don't place it unless it is placed!
                    if (x != -1 && y != -1 && z != -1)
                    {
                        te.Place(x, y, z, fireEvent: false);
                    }
                }
            }
			// *** Player ***
			
			Player = (Creature) Entities[pid];
			Explored = parsed.GetValue("explored").ToObject<HashSet<Coord>>();
			
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
			// *** Turns ***
			Turns = parsed.GetValue("turns").ToObject<TurnHandler>();
            
            Turns.Queue = Turns.QueueAsActors(parsed["turns"]["Queue"].ToObject<Queue<int>>());
			Turns.Deck = Turns.QueueAsActors(parsed["turns"]["Deck"].ToObject<Queue<int>>());
            TurnHandler.HandleVisibility();
            //			
            // *** Event Listeners ***
            var	events = parsed.GetValue("events").ToObject<Dictionary<string,Dictionary<int, string>>>();
			foreach (string type in events.Keys)
			{
				var listeners = events[type];
				listeners.Clear();
				foreach (int eid in listeners.Keys)
				{
					Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(Type.GetType("Hecatomb."+type), Entities[eid], listeners[eid]);
				}
			}
		}
	}
}
