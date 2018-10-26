/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/4/2018
 * Time: 1:13 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of FlyWeight.
	/// </summary>
	public class FlyWeight
	{
		public static Dictionary<Type, List<FlyWeight>> FlyWeightTypes = new Dictionary<Type, List<FlyWeight>>();
		// should we also index them by name?
		public int FID;
		public string Name;
		public FlyWeight()
		{
			Type t = this.GetType();
			if (!FlyWeightTypes.ContainsKey(t))
			{
				FlyWeightTypes[t] = new List<FlyWeight>();
			}
			FID = FlyWeightTypes[t].Count;
			FlyWeightTypes[t].Add(this);
		}
		
		public string Stringify()
		{
			string t = this.GetType().Name;
			string fid = FID.ToString();
			return "{\"" + t + "\": " + fid + "}";
		}
	}
}
