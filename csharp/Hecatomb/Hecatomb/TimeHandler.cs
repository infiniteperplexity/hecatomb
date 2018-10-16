/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/15/2018
 * Time: 7:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of TimeHandler.
	/// </summary>
	public class TimeHandler
	{
		decimal[] Speeds;
		int SpeedIndex = 3;
		bool PausedAfterLoad;
		bool AutoPausing;
		DateTime LastUpdate;
		
		public TimeHandler()
		{
			Speeds = new decimal[]
			{
				1/4,
				1/2,
				3/4,
				1/1,
				3/2,
				2/1,
				4/1,
				8/1
			};
			PausedAfterLoad = true;
			AutoPausing = true;
			LastUpdate = DateTime.Now;
		}
		
		public void Acted()
		{
			LastUpdate = DateTime.Now;
		}
		
		public void Update()
		{
			// this will happen almost continually
			if (PausedAfterLoad || AutoPausing)
			{
				return;
			}
			DateTime now = DateTime.Now;
			int millis = (int) now.Subtract(LastUpdate).TotalMilliseconds;
			if (millis > 1000*Speeds[SpeedIndex])
			{
				Game.Commands.AutoWait();
			}
			
		}
	}
}
