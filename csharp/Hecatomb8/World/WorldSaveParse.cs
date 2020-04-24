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
    //public class HecatombConverter : JsonConverter
    //{
    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value!.GetType().GetGenericTypeDefinition() == typeof(EntityField<>))
    //        {
    //            Type type = value!.GetType();
    //            FieldInfo fi = type.GetField("EID")!;
    //            int? eid = (int?) fi.GetValue(value!);
    //            JObject.FromObject(eid!).WriteTo(writer);
    //        }
    //        else if (value!.GetType().GetGenericTypeDefinition() == typeof(FlyWeight<>))
    //        {
    //            Type type = value!.GetType();
    //            FieldInfo fi = type.GetField("FID")!;
    //            int? fid = (int?)fi.GetValue(value!);
    //            JObject.FromObject(fid!).WriteTo(writer);
    //        }
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        JObject job = JObject.Load(reader);
    //        if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
    //        {

    //        }
    //        else if (typeof(Entity).IsAssignableFrom(objectType) && existingValue is int)
    //        { 
                
    //        }
    //        else
    //        {
    //            return existingValue!;
    //        }
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        if (typeof(Entity).IsAssignableFrom(objectType))
    //        {
    //            return true;
    //        }
    //        else if (typeof(FlyWeight<>).IsAssignableFrom(objectType))
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //}
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
                //turnQueue = turns.QueueAsIDs(turns.Queue),
                //turnDeck = turns.QueueAsIDs(turns.Deck),
                //entities = Entities,
                explored = Explored,
                //events = Events.StringifyListeners(),
                tiles = terrainFIDs,
                covers = coverFIDs

            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.Converters.Add(new HecatombConverter());
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
            //settings.Converters.Add(new HecatombConverter());
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
            // may want to double check that everything has been cleared?
            int pid = (int)parsed["player"]!;
            Entity.MaxEID = -1;
            Dictionary<int, Coord?> Placements = new Dictionary<int, Coord?>();
            //foreach (var child in parsed["entities"]!.Values())
            //{
            //    // will this handle Player?  It's such a terrible way I do that...
            //    string t = (string)child["ClassName"];
            //    Type T = Type.GetType("Hecatomb." + t);
            //    Entity ge = (Entity)child.ToObject(T);
            //    Entities[ge.EID] = ge;
            //    Entity.MaxEID = Math.Max(Entity.MaxEID, ge.EID);
            //    ge.Spawned = true;

            //    if (ge is StateHandler)
            //    {
            //        if (ge is TurnHandler)
            //        {
            //            var th = (TurnHandler)ge;
            //            th.Queue = th.QueueAsActors(parsed["turnQueue"].ToObject<Queue<int>>());
            //            th.Deck = th.QueueAsActors(parsed["turnDeck"].ToObject<Queue<int>>());
            //        }
            //        (ge as StateHandler).Activate();
            //    }
            //}
            // okay, this gets weird...so...
            foreach (int eid in Placements.Keys.ToList())
            {
                Entity e = Entities[eid];
                if (e is TileEntity)
                {
                    TileEntity te = (TileEntity)e;

                    Coord? c = Placements[eid];
                    if (c != null)
                    {
                        // we need to place without firing events
                        te.PlaceInValidEmptyTile(((Coord)c).X, ((Coord)c).Y, ((Coord)c).Z);
                    }
                }
            }
            //ValidateOutdoors();
            // *** Player ***

            Player = (Creature)Entities[pid];
            Explored = parsed.GetValue("explored")!.ToObject<HashSet<Coord>>()!;



            // *** Event Listeners ***
            var events = parsed.GetValue("events")!.ToObject<Dictionary<string, Dictionary<int, string>>>();
            //foreach (string type in events!.Keys)
            //{
            //    var listeners = events[type];
            //    if (Events.ListenerTypes.ContainsKey(type))
            //    {
            //        Events.ListenerTypes[type].Clear();
            //    }
            //    else
            //    {
            //        Events.ListenerTypes[type] = new Dictionary<int, Func<GameEvent, GameEvent>>();
            //    }
            //    foreach (int eid in listeners.Keys)
            //    {
            //        Events.ListenerTypes[type][eid] = (Func<GameEvent, GameEvent>)Delegate.CreateDelegate(typeof(Func<GameEvent, GameEvent>), Entities[eid], listeners[eid]);
            //    }
            //}
            InterfaceState.HandlePlayerVisibility();
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
            //if (Game.Options.ReseedRandom)
            //{
            //    int r = System.DateTime.Now.Millisecond;
            //    Game.World.Random = new StatefulRandom(r);
            //    Debug.WriteLine($"Changed random seed to {r}");
            //}
        }
    }
}

