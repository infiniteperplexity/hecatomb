/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/4/2018
 * Time: 1:13 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of FlyWeight.
	/// </summary>
	public class FlyWeight
	{
		public static Dictionary<Type, List<FlyWeight>> Enumerated = new Dictionary<Type, List<FlyWeight>>();
		public static Dictionary<Type, Dictionary<string, FlyWeight>> Types = new Dictionary<Type, Dictionary<string, FlyWeight>>();
		// should we also index them by name?
		public int FID;
        public string TypeName;
		public string Name;
        public FlyWeight(string type, string name)
        {
            TypeName = type;
            Name = name;
            Type t = this.GetType();
            if (!Enumerated.ContainsKey(t))
            {
                Enumerated[t] = new List<FlyWeight>();
            }
            FID = Enumerated[t].Count;
            Enumerated[t].Add(this);
            if (!Types.ContainsKey(t))
            {
                Types[t] = new Dictionary<string, FlyWeight>();
            }
			Types[t][TypeName] = this;
		}
	}
}
