using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Hecatomb8
{
    // FlyWeights are sometimes used as the keys for dictionaries, and that confuses the serializer
    // But it works fine if you make them the keys of a Dictionary subclass with the JsonArrayAttribute
    [JsonArrayAttribute]
    public class JsonArrayDictionary<T, S> : Dictionary<T, S> where T : notnull
    {
        public JsonArrayDictionary(IDictionary<T, S> i) : base(i)
        {

        }

        public JsonArrayDictionary() : base()
        {

        }
    }

    // FlyWeights represent game entities for which every "instance" has exactly the same properties
    // A self-referencing generic class allows something resembling static inheritance
    public class FlyWeight<T> where T : FlyWeight<T>
    {
        // FlyWeights are numbered for efficient serialization
        public int FID;
        public static List<T> Enumerated = new List<T>();
        // FlyWeights are indexed by string for convenient access
        public string TypeName;

        public FlyWeight(string s)
        {
            // newly-initialized FlyWeights automatically index themselves
            FID = Enumerated.Count;
            Enumerated.Add((T)this);
            TypeName = s;
        }

        public static T GetByNumber(int i)
        {
            return Enumerated[i];
        }

        static FlyWeight()
        {
            // FlyWeight instances are stored as static members of the derived FlyWeight class
            // the static constructor uses reflection to make sure they are initialized before the indexers are accessed
            // this may or may not be the best way of doing things
            foreach (FieldInfo f in typeof(T).GetFields())
            {
                if (f.IsStatic)
                {
                    f.GetValue(null);
                }
            }
        }
    }
}