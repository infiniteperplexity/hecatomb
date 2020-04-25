using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class HecatombConverter : JsonConverter
    {
        static JsonSerializer SubSerializer;
        static HecatombConverter()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new FlyWeightConverter());
            SubSerializer = JsonSerializer.Create(settings);
        }
        public static void Test()
        {
            //var settings = new JsonSerializerSettings();
            //settings.Converters.Add(new HecatombConverter());
            //JObject j = JObject.FromObject(Player!);
            //Debug.WriteLine(j);
            //var c = JsonConvert.DeserializeObject<Entity>(j.ToString(), settings)!;
            //Debug.WriteLine(c);
            //var p = JsonConvert.DeserializeObject<Necromancer>(j.ToString(), settings)!;
            //Debug.WriteLine(p.GetComponent<SpellCaster>().EID);
            //Debug.WriteLine(p.coverTest);
            //Debug.WriteLine(Object.ReferenceEquals(p.coverTest, Cover.Water));
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JObject.FromObject(value!).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                int fid = (int)job["FID"]!;
                var method = objectType.GetMethod("GetByNumber")!;
                var fw = method.Invoke(null, new object[] { fid })!;
                return fw;
            }
            else if (typeof(Entity).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                string s = (string)job["Class"]!;
                Type t = Type.GetType("Hecatomb8." + s)!;
                object ent = SubSerializer.Deserialize(new JTokenReader(job), t)!;
                return ent;
            }
            else
            {
                return existingValue!;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            if (typeof(Entity).IsAssignableFrom(objectType))
            {
                return true;
            }
            else if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }
    }

    public class FlyWeightConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JObject.FromObject(value!).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                int fid = (int)job["FID"]!;
                var method = objectType.GetMethod("GetByNumber")!;
                var fw = method.Invoke(null, new object[] { fid })!;
                return fw;
            }
            else
            {
                return existingValue!;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }
    }
    public partial class World
    {
        public void Stringify()
        {
            int[,,] terrainFIDs = new int[Width, Height, Depth];
            int[,,] coverFIDs = new int[Width, Height, Depth];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        terrainFIDs[i, j, k] = Terrains.GetWithBoundsChecked(i, j, k).FID;
                        coverFIDs[i, j, k] = Covers.GetWithBoundsChecked(i, j, k).FID;
                    }
                }
            }
            var turns = GetState<TurnHandler>();
            var jsonready = new
            {
                buildDate = GameManager.BuildDate.ToString(),
                random = Random,
                player = Player!.EID,
                entities = Entities,
                explored = Explored,
                events = Events.StringifyListeners(),
                tiles = terrainFIDs,
                covers = coverFIDs

            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;

            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            using (TextWriter writer = File.CreateText(path + @"\saves\" + GameManager.GameName + ".json"))
            {
                var serializer = JsonSerializer.Create(settings);
                serializer.Serialize(writer, jsonready);
            }
        }

        public void Parse(string filename)
        {
            //Reset();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            string json = System.IO.File.ReadAllText(path + @"\saves\" + GameManager.GameName + ".json");
            JObject parsed;
            using (StreamReader stream = File.OpenText(filename))
            using (JsonTextReader reader = new JsonTextReader(stream))
            {
                JsonSerializer serializer = JsonSerializer.Create(settings);
                parsed = (JObject)serializer.Deserialize(reader)!;
            }
            // *** Random Seed ***
            Random = parsed["random"]!.ToObject<StatefulRandom>()!;
            Random.Initialize();
            // *** Terrains and Covers ***
            int[,,] tiles = parsed["tiles"]!.ToObject<int[,,]>()!;
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    for (int k = 0; k < tiles.GetLength(2); k++)
                    {
                        Terrains.SetWithBoundsChecked(i, j, k, Terrain.Enumerated[tiles[i, j, k]]!);
                    }
                }
            }
            int[,,] covers = parsed["covers"]!.ToObject<int[,,]>()!;
            for (int i = 0; i < covers.GetLength(0); i++)
            {
                for (int j = 0; j < covers.GetLength(1); j++)
                {
                    for (int k = 0; k < covers.GetLength(2); k++)
                    {
                        Covers.SetWithBoundsChecked(i, j, k, Cover.Enumerated[covers[i, j, k]]!);
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
            Entity.MaxEID = -1;
            // may want to double check that everything has been cleared?
            int pid = (int)parsed["player"]!;
            Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
            foreach (var child in parsed["entities"]!.Values())
            {
                var e = JsonConvert.DeserializeObject<Entity>(child.ToString(), settings)!;
                Entities[(int)e!.EID!] = e;
                if (e is TileEntity)
                {
                    TileEntity te = (TileEntity)e;
                    Coord c = (Coord)te._coord!;
                    if (c != null)
                    {
                        // we need to place without firing events...why do we fire events in the first place?  maybe that should only be for more specific types of placement
                        te.PlaceInValidEmptyTile(((Coord)c).X, ((Coord)c).Y, ((Coord)c).Z);
                    }
                }
                Entity.MaxEID = Math.Max(Entity.MaxEID, (int)e.EID);
                // we might have to do some sort of reactivation for statehandlers but I hope not
            }
            //ValidateOutdoors();
            // *** Player ***
            Player = (Creature)Entities[pid];
            Explored = parsed.GetValue("explored")!.ToObject<HashSet<Coord>>()!;
            // *** Event Listeners ***
            var events = parsed.GetValue("events")!.ToObject<Dictionary<string, Dictionary<int, string>>>();
            foreach (string type in events!.Keys)
            {
                var listeners = events[type];
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
                    Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>)Delegate.CreateDelegate(typeof(Func<GameEvent, GameEvent>), Entities[eid], listeners[eid]);
                }
            }
            InterfaceState.PlayerIsReady();
            //if (Game.Options.ReseedRandom)
            //{
            //    int r = System.DateTime.Now.Millisecond;
            //    Game.World.Random = new StatefulRandom(r);
            //    Debug.WriteLine($"Changed random seed to {r}");
            //}
        }
    }
}

