using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;
using System.IO.Compression;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class HecatombConverter : JsonConverter
    {
        static JsonSerializer SubSerializer;
        static HecatombConverter()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new SubConverter());
            SubSerializer = JsonSerializer.Create(settings);
        }
        public static void Test()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            var j = JsonConvert.SerializeObject(Player.GetComponent<SpellCaster>(), settings);
            Debug.WriteLine(j);
            var c = JsonConvert.DeserializeObject<SpellCaster>(j.ToString(), settings)!;
            //var p = JsonConvert.DeserializeObject<Necromancer>(j.ToString(), settings)!;
            //Debug.WriteLine(p.GetComponent<SpellCaster>().EID);
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is List<Type>)
            {
                var list = new List<string>();
                foreach (Type t in (value as List<Type>)!)
                {
                    list.Add(t.Name);
                }
                serializer.Serialize(writer, list);
            }
            else
            {
                JObject.FromObject(value!, SubSerializer).WriteTo(writer);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (typeof(FlyWeightParent).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                int fid = (int)job["FID"]!;
                return FlyWeightParent.LookupTable[objectType][fid];
            }
            else if (typeof(Entity).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                string s = (string)job["Class"]!;
                //Debug.WriteLine(s);
                Type t = Type.GetType("Hecatomb8." + s)!;
                object ent = SubSerializer.Deserialize(new JTokenReader(job), t)!;
                return ent;
            }
            else if (typeof(List<Type>).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                List<string> slist = JsonConvert.DeserializeObject<List<string>>(job.ToString());
                var tlist = new List<Type>();
                foreach (string s in slist)
                {
                    tlist.Add(Type.GetType("Hecatomb8." + s)!);
                }
                return tlist;
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
            else if (typeof(FlyWeightParent).IsAssignableFrom(objectType))
            {
                return true;
            }
            else if (typeof(List<Type>).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }
    }

    public class SubConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is List<Type>)
            {
                var list = new List<string>();
                foreach (Type t in (value as List<Type>)!)
                {
                    list.Add(t.Name);
                }
                serializer.Serialize(writer, list);
            }
            else
            {
                JObject.FromObject(value!).WriteTo(writer);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (typeof(FlyWeightParent).IsAssignableFrom(objectType))
            {
                JObject job = JObject.Load(reader);
                int fid = (int)job["FID"]!;
                return FlyWeightParent.LookupTable[objectType][fid];
            }
            else if (typeof(List<Type>).IsAssignableFrom(objectType))
            {
                JToken job = JToken.Load(reader);
                List<string> slist = JsonConvert.DeserializeObject<List<string>>(job.ToString());
                var tlist = new List<Type>();
                foreach (string s in slist)
                {
                    tlist.Add(Type.GetType("Hecatomb8." + s)!);
                }
                return tlist;
            }
            else
            {
                return existingValue!;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            if (typeof(FlyWeightParent).IsAssignableFrom(objectType))
            {
                return true;
            }
            else if (typeof(List<Type>).IsAssignableFrom(objectType))
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
            //FlyWeightParent.Touch();
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
                handlers = StateHandlers,
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
            //using (TextWriter writer = File.CreateText(path + @"\saves\" + GameManager.GameName + ".json"))
            //{
            //    var serializer = JsonSerializer.Create(settings);
            //    serializer.Serialize(writer, jsonready);
            //}

            if (File.Exists(path + @"\saves\" + GameManager.GameName + ".zip"))
            {
                File.Move(path + @"\saves\" + GameManager.GameName + ".zip", path + @"\saves\" + GameManager.GameName + "_temp");
            }
            try
            {
                using (FileStream zipToOpen = new FileStream(path + @"\saves\" + GameManager.GameName + ".zip", FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        ZipArchiveEntry save = archive.CreateEntry("save.json");
                        using (StreamWriter writer = new StreamWriter(save.Open()))
                        {
                            var serializer = JsonSerializer.Create(settings);
                            serializer.Serialize(writer, jsonready);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (File.Exists(path + @"\saves\" + GameManager.GameName + "_temp"))
                {
                    File.Move(path + @"\saves\" + GameManager.GameName + "_temp", path + @"\saves\" + GameManager.GameName + ".zip");
                }
            }
            if (File.Exists(path + @"\saves\" + GameManager.GameName + "_temp"))
            {
                File.Delete(path + @"\saves\" + GameManager.GameName + "_temp");
            }
            //using (TextWriter writer = File.CreateText(path + @"\saves\" + GameManager.GameName + @"\save.json"))
            //{
            //    var serializer = JsonSerializer.Create(settings);
            //    serializer.Serialize(writer, jsonready);  
            //}
            //// if the zipped saved game already exists, moved it out of the way
            //if (File.Exists(path + @"\saves\" + GameManager.GameName + ".zip"))
            //{
            //    File.Move(path + @"\saves\" + GameManager.GameName + ".zip", path + @"\saves\" + GameManager.GameName + "_temp.zip");
            //}
            //try
            //{
            //    ZipFile.CreateFromDirectory(path + @"\saves\" + GameManager.GameName, path + @"\saves\" + GameManager.GameName + ".zip");

            //}
            //catch(Exception e)
            //{
            //    // if we failed, try to bring the old one back
            //    if (File.Exists(path + @"\saves\" + GameManager.GameName + "_temp.zip"))
            //    {
            //        File.Move(path + @"\saves\" + GameManager.GameName + "_temp.zip", path + @"\saves\" + GameManager.GameName + ".zip");
            //    }
            //    ExceptionHandling.Handle(e);
            //}
            //// get rid of the old zipped directory
            //if (File.Exists(path + @"\saves\" + GameManager.GameName + "_temp.zip"))
            //{
            //    File.Delete(path + @"\saves\" + GameManager.GameName + "_temp.zip");
            //}
            //// get rid of the unzipped directory
            //Directory.Delete(path + @"\saves\" + GameManager.GameName, true);

        }

        public void Parse()
        {
            var name = GameManager.GameName;
            Activity.Touch();
            Cover.Touch();
            Research.Touch();
            Resource.Touch();
            Species.Touch();
            Team.Touch();
            Terrain.Touch();
            //Reset();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HecatombConverter());
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            JObject parsed;
            using (FileStream zipToOpen = new FileStream(path + @"\saves\" + GameManager.GameName + ".zip", FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry save = archive.GetEntry("save.json");
                    using (var stream = new StreamReader(save.Open()))
                    {
                        using (JsonTextReader reader = new JsonTextReader(stream))
                        {
                            JsonSerializer serializer = JsonSerializer.Create(settings);
                            parsed = (JObject)serializer.Deserialize(reader)!;
                        }
                    }
                }
            }
            //ZipFile.ExtractToDirectory(path + @"\saves\" + name + ".zip", path + @"\saves\" + name);       
            //JObject parsed;
            //using (StreamReader stream = File.OpenText(path + @"\saves\" + name + @"\save.json"))
            
            //Directory.Delete(path + @"\saves\" + name, true);
            // *** Random Seed ***
            Random = parsed["random"]!.ToObject<StatefulRandom>()!;
            Random.Reinitialize();
            StateHandlers = parsed["handlers"]!.ToObject<Dictionary<string, int>>()!;
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
            }
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
            Events.Suppressed = true;
            foreach (var e in Entities.Values)
            { 
                if (e is TileEntity)
                {
                    TileEntity te = (TileEntity)e;
                    // Items in particular are not always placed
                    Coord? c = te._coord;
                    if (c != null)
                    {
                        //Debug.WriteLine("placing " + e.Class);
                        te.PlaceInValidEmptyTile(((Coord)c).X, ((Coord)c).Y, ((Coord)c).Z);
                    }
                }
                Entity.MaxEID = Math.Max(Entity.MaxEID, (int)e.EID!);
                // we might have to do some sort of reactivation for statehandlers but I hope not
            }
            Events.Suppressed = false;
            //ValidateOutdoors();
            // *** Player ***
            Player = (Creature)Entities[pid];
            Explored = parsed.GetValue("explored")!.ToObject<HashSet<Coord>>()!;
            // *** Event Listeners ***

            ValidateOutdoors();
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

