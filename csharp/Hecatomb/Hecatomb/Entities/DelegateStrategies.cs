/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 10:13 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;


namespace Hecatomb
{
	/// <summary>
	/// Description of GameStrategies.
	/// </summary>
	public class DelegateStrategies
	{
		string[] StrategyNames;
		Dictionary<string, Dictionary<string, Action>> Strategies;
		
		public DelegateStrategies()
		{
			StrategyNames = new [] {"AfterSelfPlace"};
			foreach (string s in StrategyNames)
			{
				Strategies[s] = new Dictionary<string, Action>();
			}
//			Strategies["AfterSelfPlace"]["AfterRampPlace"] = (TypedEntity t, int x, int y, int z) => {
//				Feature ramp = (Feature) t;
//				ramp.Remove();
//				Debug.WriteLine("let's pretend we just built a slope.");
//			};
		}
	}
}
