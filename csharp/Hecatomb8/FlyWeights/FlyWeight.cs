using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

// so...we want to get rid of text keys entirely
namespace Hecatomb8
{
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