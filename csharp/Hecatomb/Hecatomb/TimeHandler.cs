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
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of TimeHandler.
	/// </summary>
	public class TimeHandler
	{
		decimal[] Speeds;
		int SpeedIndex = 3;
		public bool PausedAfterLoad;
		public bool AutoPausing;
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
			PausedAfterLoad = false;
		}
		
		public void Update()
		{
			// this will happen almost continually
			foreach (ParticleEmitter e in Game.MainPanel.Emitters.ToList())
			{
				e.Update();
			}
			foreach (Particle p in Game.MainPanel.Particles.ToList())
			{	
				p.Update();
			}
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
