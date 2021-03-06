﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/9/2018
 * Time: 2:27 PM
 */
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hecatomb
{
    public struct GlyphFields
    {
        public char Symbol;
        public string FG;
        public string BG;
    }
    public class HecatombConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TypedEntity /*&& (value as TileEntity).Distinctive*/)
            {
                TypedEntity te = (TypedEntity)value;
                EntityType et = EntityType.Types[te.TypeName];
                JObject o = JObject.FromObject(value);
                if (te.Symbol!=et.Symbol)
                {
                    o.Add("Symbol", te.Symbol);
                }
                if (te.FG != et.FG)
                {
                    o.Add("FG", te.FG);
                }
                if (te.BG != et.BG)
                {
                    o.Add("BG", te.BG);
                }
                o.WriteTo(writer);
            }
            else
            {
                JObject.FromObject(value).WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject job = JObject.Load(reader);
            string t = (string)job["ClassName"];
            Type T = Type.GetType("Hecatomb." + t);
            Entity ge = (Entity)job.ToObject(T);
            if (ge is TypedEntity)
            {
                TypedEntity te = (TypedEntity)ge;
                EntityType.Types[te.TypeName].Standardize(te);
                GlyphFields gf = job.ToObject<GlyphFields>();
                if (gf.Symbol!=default(char))
                {
                    te.Symbol = gf.Symbol;
                }
                if (gf.FG!=null)
                {
                    te.FG = gf.FG;
                }
                if (gf.BG != null)
                {
                    te.BG = gf.BG;
                }
            }
            return ge;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Entity).IsAssignableFrom(objectType);
        }
    }

    public partial class World
	{

        //https://blog.maskalik.com/asp-net/json-net-implement-custom-serialization/
        public void Stringify()
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
            var turns = GetState<TurnHandler>();
            var jsonready = new
            {
                buildDate = Game.BuildDate.ToString(),
                random = Random,
                player = Player.EID,
                turnQueue = turns.QueueAsIDs(turns.Queue),
                turnDeck = turns.QueueAsIDs(turns.Deck),
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
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;

            // okay...this could be a circular reference or else a string that's just too big
            //var json = JsonConvert.SerializeObject(jsonready, settings);

            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");           
            using (TextWriter writer = File.CreateText(path + @"\saves\" + Game.GameName + ".json"))
            {
                var serializer = JsonSerializer.Create(settings);
                serializer.Serialize(writer, jsonready);
            }

            //System.IO.File.WriteAllText(path + @"\saves\" + Game.GameName + ".json", json);
            //return json;
		}

    //public void Parse(string json)
    public void Parse(string filename)	
		{
            Reset();
            TheFixer.Purge();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            //JObject parsed;
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            string json = System.IO.File.ReadAllText(path + @"\saves\" + Game.GameName + ".json");
            //JObject parsed = (JObject) JsonConvert.DeserializeObject(json, settings);
            JObject parsed;
            using (StreamReader stream = File.OpenText(filename))
            using (JsonTextReader reader = new JsonTextReader(stream))
            {
                JsonSerializer serializer = JsonSerializer.Create(settings);
                parsed = (JObject)serializer.Deserialize(reader);
            }
            //var serializer = new JsonSerializer();
            // *** Random Seed ***
            Random = parsed["random"].ToObject<StatefulRandom>();
			Random.Initialize();
            // *** Terrains and Covers ***
            int[,,] tiles = parsed["tiles"].ToObject<int[,,]>();
            //int[,,] covers = parsed["covers"].ToObject<int[,,]>();
            for (int i=0; i<tiles.GetLength(0); i++)
			{
				for (int j=0; j<tiles.GetLength(1); j++)
				{
					for (int k=0; k<tiles.GetLength(2); k++)
					{
						Terrains[i,j,k] = Terrain.Enumerated[tiles[i,j,k]];
                    }
				}
			}
            //int[,,] tiles = parsed["tiles"].ToObject<int[,,]>();
            int[,,] covers = parsed["covers"].ToObject<int[,,]>();
            for (int i = 0; i < covers.GetLength(0); i++)
            {
                for (int j = 0; j < covers.GetLength(1); j++)
                {
                    for (int k = 0; k < covers.GetLength(2); k++)
                    {
                        Covers[i, j, k] = Cover.Enumerated[covers[i, j, k]];
                    }
                }
            }
            // *** Entities ***
            int tally = 0;
            foreach (int eid in Entities.Keys.ToList())
			{
                // this process can trigger recursive despawning
                if (Entities.ContainsKey(eid))
                {
                    tally += 1;
                    Entities[eid].Despawn();
                }
			}
            // may want to double check that everything has been cleared?
			int pid = (int) parsed["player"];
			Entity.MaxEID = -1;
            Dictionary<int, Coord> Placements = new Dictionary<int, Coord>();
            Dictionary<int, GlyphFields> Glyphs = new Dictionary<int, GlyphFields>();
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
                    Glyphs[ge.EID] = child.ToObject<GlyphFields>();
                }
				Entities[ge.EID] = ge;
				Entity.MaxEID = Math.Max(Entity.MaxEID, ge.EID);
				ge.Spawned = true;

                if (ge is StateHandler)
                {
                    if (ge is TurnHandler)
                    {
                        var th = (TurnHandler)ge;
                        th.Queue = th.QueueAsActors(parsed["turnQueue"].ToObject<Queue<int>>());
                        th.Deck = th.QueueAsActors(parsed["turnDeck"].ToObject<Queue<int>>());
                    }
                    (ge as StateHandler).Activate();
                }
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
                        // will this sometimes do the wrong thing?
                        EntityType.Types[tye.TypeName].Standardize(tye);
                        GlyphFields gf = Glyphs[eid];
                        if (gf.Symbol!=default(char))
                        {
                            tye.Symbol = gf.Symbol;
                        }
                        if (gf.FG != null)
                        {
                            tye.FG = gf.FG;
                        }
                        if (gf.BG != null)
                        {
                            tye.BG = gf.BG;
                        }
                    }
                    //if (te is Structure)
                    //{
                        // structures need some cleaning up
                        //Structure s = (Structure)te;
                        // why did I do this again?
                        //int size = s.Width * s.Height;
                        //s.Features = s.Features.Skip(size).ToList();
                    //}
                    // don't place it unless it is placed!
                    if (x != -1 && y != -1 && z != -1)
                    {
                        te.Place(x, y, z, fireEvent: false);
                    }
                }
            }
            ValidateOutdoors();
			// *** Player ***
			
			Player = (Creature) Entities[pid];
			Explored = parsed.GetValue("explored").ToObject<HashSet<Coord>>();

            
    
            // *** Event Listeners ***
            var	events = parsed.GetValue("events").ToObject<Dictionary<string,Dictionary<int, string>>>();
			foreach (string type in events.Keys)
			{
                var listeners = events[type];
                //listeners.Clear();
                if (Events.ListenerTypes.ContainsKey(type))
                {
                    Events.ListenerTypes[type].Clear();
                }
                else
                {
                    Events.ListenerTypes[type] = new Dictionary<int, Func<GameEvent, GameEvent>>();
                }
				foreach (int eid in listeners.Keys)
				{
                    Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>) Delegate.CreateDelegate(typeof(Func<GameEvent, GameEvent>), Entities[eid], listeners[eid]);
				}
			}
            TurnHandler.HandleVisibility();
            InterfacePanel.DirtifyUsualPanels();
            World.WorldSafeToDraw = true;
            if (Game.Options.ReseedRandom)
            {
                int r = System.DateTime.Now.Millisecond;
                Game.World.Random = new StatefulRandom(r);
                Debug.WriteLine($"Changed random seed to {r}");
            }
        }
	}
}
