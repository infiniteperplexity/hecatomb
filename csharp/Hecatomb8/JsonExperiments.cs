using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Reflection;

namespace Hecatomb8
{

    public static class JsonExperiment
    {
        public static void Bar()
        {
            var a = new Foo(2);
            a.MyFoo = new Foo(3);

            string json = (JsonConvert.SerializeObject(a));      
            //string json = JsonConvert.SerializeObject(a, Converter1.Settings);
            Debug.WriteLine(json);
        }

        public static void Foo()
        {

        }
    }

    [JsonConverter(typeof(Converter1))]
    public class Foo
    {
        public int Number;
        [JsonIgnore]
        public int Numeral = 0;
        public Foo? MyFoo;

        public Foo(int n)
        {
            Number = n;
            
        }
    }
    public class Converter1 : JsonConverter
    {
        public static Converter1 Converter = new Converter1();

        public static JsonSerializerSettings Settings;
        public static JsonSerializerSettings Settings2;
        public static JsonSerializer Serializer;

        static Converter1()
        {
            
            Settings = new JsonSerializerSettings();
            Settings.Converters.Add(Converter1.Converter);
            Settings2 = new JsonSerializerSettings();
            Settings2.NullValueHandling = NullValueHandling.Ignore;
            Serializer = JsonSerializer.Create(Settings2);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            Debug.WriteLine($"value is {value}");
            Type type = value!.GetType();
            Debug.WriteLine("loop through");
            foreach (var prop in type.GetFields())
            {
                Debug.WriteLine("looping through");
                Debug.WriteLine(prop);
                var attr = Attribute.GetCustomAttributes(prop);
                foreach (var a in attr)
                {
                    if (a is Newtonsoft.Json.JsonIgnoreAttribute)
                    {
                        Debug.WriteLine("why yes it is.");
                    }
                }
                
            }
            JObject.FromObject(value!, Serializer).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject job = JObject.Load(reader);
            return job;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }


    //public class HecatombConverter : JsonConverter
    //{
    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        JObject.FromObject(value).WriteTo(writer);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        // Load JObject from stream
    //        JObject job = JObject.Load(reader);
    //        string t = (string)job["ClassName"];
    //        Type T = Type.GetType("Hecatomb." + t);
    //        Entity ge = (Entity)job.ToObject(T);
    //        if (ge is ComposedEntity)
    //        {
    //            ComposedEntity te = (ComposedEntity)ge;
    //            EntityType.Types[te.TypeName].Standardize(te);
    //            GlyphFields gf = job.ToObject<GlyphFields>();
    //            if (gf.Symbol != default(char))
    //            {
    //                te.Symbol = gf.Symbol;
    //            }
    //            if (gf.FG != null)
    //            {
    //                te.FG = gf.FG;
    //            }
    //            if (gf.BG != null)
    //            {
    //                te.BG = gf.BG;
    //            }
    //        }
    //        return ge;
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return typeof(Entity).IsAssignableFrom(objectType);
    //    }
    //}
}
/*
 
 So, it seems that this is done using a Converter, which is part of the settings for a JsonSerializer.


public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
{
    JObject jo = new JObject();
    Type type = value.GetType();
    jo.Add("type", type.Name);

    foreach (PropertyInfo prop in type.GetProperties())
    {
        if (prop.CanRead)
        {
            object propVal = prop.GetValue(value, null);
            if (propVal != null)
            {
                jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
            }
        }
    }
    jo.WriteTo(writer);
}


So I think the deal is, you could have multiple Converters with different CanConvert methods...but...okay...so you need to be able to mess with the properties while serializing...

Okay...I think I understand how this is done.  I would create two sets of converters and settings.
But...this recursive stuff, I dunno...let's leave off this for a  bit
so I think you have to traverse the stupid thing manually.

    https://stackoverflow.com/questions/29267030/custom-jsonconverter-writejson-does-not-alter-serialization-of-sub-properties

 */
