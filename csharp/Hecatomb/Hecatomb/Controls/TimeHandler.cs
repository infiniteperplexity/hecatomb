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
		//decimal[] Speeds;
        List<(int, int)> Speeds;
		int SpeedIndex = 3;
		public bool PausedAfterLoad;
		public bool AutoPausing;
        public bool Frozen;
		DateTime LastUpdate;
		
		public TimeHandler()
		{
            Speeds = new List<(int, int)>
            {
                (1,4),
                (1,2),
                (3,4),
                (1,1),
                (3,2),
                (2,1),
                (4,1),
                (8,1)
            };
            PausedAfterLoad = true;
			AutoPausing = true;
			LastUpdate = DateTime.Now;
		}

        public void SlowDown()
        {
            if (SpeedIndex < Speeds.Count - 1)
            {
                SpeedIndex += 1;
                int num = Speeds[SpeedIndex].Item2;
                int denom = Speeds[SpeedIndex].Item1;
                Game.StatusPanel.PushMessage($"Game speed decreased to {num}:{denom}");
                InterfacePanel.DirtifySidePanels();
            }
        }

        public void SpeedUp()
        {
            if (SpeedIndex > 0)
            {
                SpeedIndex -= 1;
                int num = Speeds[SpeedIndex].Item2;
                int denom = Speeds[SpeedIndex].Item1;
                Game.StatusPanel.PushMessage($"Game speed increased to {num}:{denom}");
                InterfacePanel.DirtifySidePanels();
            }
        }
		
		public void Acted()
		{
			LastUpdate = DateTime.Now;
			PausedAfterLoad = false;
		}

		public void Update()
		{
            if (Frozen)
            {
                return;
            }
			// this will happen almost continually
			foreach (ParticleEmitter e in Game.World.Emitters.ToList())
			{
				e.Update();
			}
			foreach (Particle p in Game.World.Particles.ToList())
			{	
				p.Update();
			}
			if (PausedAfterLoad || AutoPausing)
			{
				return;
			}
			DateTime now = DateTime.Now;
			int millis = (int) now.Subtract(LastUpdate).TotalMilliseconds;
            decimal fraction = (decimal) Speeds[SpeedIndex].Item1 / (decimal) Speeds[SpeedIndex].Item2;
            if (millis > 1000*fraction)
			{
				Game.Commands.AutoWait();
			}
			
		}
	}
}
