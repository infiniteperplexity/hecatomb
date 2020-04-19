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

        //// I don't think we actually need this after getting rid of the string keys 
        //static FlyWeight()
        //{
        //    foreach (FieldInfo f in typeof(T).GetFields())
        //    {
        //        if (f.IsStatic)
        //        {
        //            f.GetValue(null);
        //        }
        //    }
        //}
    }
}
