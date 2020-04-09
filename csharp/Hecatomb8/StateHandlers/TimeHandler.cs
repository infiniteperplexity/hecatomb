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
        List<(int, int, string)> Speeds;
		int SpeedIndex = 3;
		public bool PausedAfterLoad;
		public bool AutoPausing;
        public bool Frozen;
		DateTime LastUpdate;
		
		public TimeHandler()
		{
            Speeds = new List<(int, int, string)>
            {
                (1,4, "4x"),
                (1,2, "2x"),
                (3,4, "4/3x"),
                (1,1, "1x"),
                (3,2, "2/3x"),
                (2,1, "1/2x"),
                (4,1, "1/4x"),
                (8,1, "1/8x")
            };
            PausedAfterLoad = true;
			AutoPausing = true;
			LastUpdate = DateTime.Now;
		}

        public List<ColoredText> GetTimeText()
        {
            var list = new List<ColoredText>();
            var t = Game.World.Turns;
            // should probably show the speed when unpaused
            list.Add((Game.Time.AutoPausing || Game.Time.PausedAfterLoad) ? "{yellow}Paused" : "{yellow}Speed: " + Speeds[SpeedIndex].Item3);
            string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
            //string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
            list.Add(time);
            return list;
        }
        public void SlowDown()
        {
            if (SpeedIndex < Speeds.Count - 1)
            {
                SpeedIndex += 1;
                int num = Speeds[SpeedIndex].Item2;
                int denom = Speeds[SpeedIndex].Item1;
                Game.InfoPanel.PushMessage($"Game speed decreased to {num}:{denom}");
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
                Game.InfoPanel.PushMessage($"Game speed increased to {num}:{denom}");
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
			if (PausedAfterLoad || AutoPausing || Game.Controls.AlwaysPaused)
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
