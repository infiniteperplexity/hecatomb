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
		public string[] StrategyNames;
		public Dictionary<string, Dictionary<string, Action<PositionedEntity, int, int, int>>> Strategies;
		
		public DelegateStrategies()
		{
			StrategyNames = new [] {"AfterSelfPlace"};
			foreach (string s in StrategyNames)
			{
				Strategies[s] = new Dictionary<string, Action<PositionedEntity, int, int, int>>();
			}
			Strategies["AfterSelfPlace"]["AfterRampPlace"] = (PositionedEntity t, int x, int y, int z) => {
				Feature ramp = (Feature) t;
				ramp.Remove();
                Game.World.Covers[x, y, z] = Cover.NoCover;
                Game.World.Tiles[x, y, z] = Terrain.UpSlopeTile;
                Game.World.Tiles[x, y, z + 1] = Terrain.DownSlopeTile;
			};
		}
	}
}
