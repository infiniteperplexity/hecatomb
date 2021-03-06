﻿
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public static class Time
    {
        //decimal[] Speeds;
        static List<(int numerator, int denominator, string display)> Speeds;
        static int SpeedIndex = 3;
        //static bool PausedAfterLoad;
        static public bool AutoPausing;
        static public bool Frozen;
        public static DateTime LastUpdate;

        static Time()
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
            //PausedAfterLoad = true;
            AutoPausing = true;
            LastUpdate = DateTime.Now;
        }

        public static List<ColoredText> GetTimeText()
        {
            var list = new List<ColoredText>();
            var t = GameState.World!.GetState<TurnHandler>();
            // should probably show the speed when unpaused
            list.Add((Time.AutoPausing /*|| Time.PausedAfterLoad*/) ? "{yellow}Paused" : "{yellow}Speed: " + Speeds[SpeedIndex].display);
            string time = "\u263C " + t.Day.ToString().PadLeft(4, '0') + ':' + t.Hour.ToString().PadLeft(2, '0') + ':' + t.Minute.ToString().PadLeft(2, '0');
            list.Add(time);
            return list;
        }
        public static void SlowDown()
        {
            if (SpeedIndex < Speeds.Count - 1)
            {
                SpeedIndex += 1;
                int num = Speeds[SpeedIndex].Item2;
                int denom = Speeds[SpeedIndex].Item1;
                PushMessage($"Game speed decreased to {num}:{denom}");
                InterfaceState.DirtifyTextPanels();
            }
        }

        public static void SpeedUp()
        {
            if (SpeedIndex > 0)
            {
                SpeedIndex -= 1;
                int num = Speeds[SpeedIndex].Item2;
                int denom = Speeds[SpeedIndex].Item1;
                PushMessage($"Game speed increased to {num}:{denom}");
                InterfaceState.DirtifyTextPanels();
            }
        }

        public static void Update()
        {
            if (Frozen)
            {
                return;
            }
            // this will happen almost continually
            foreach (ParticleEmitter e in InterfaceState.Emitters.ToList())
            {
                e.Update();
            }
            foreach (var p in InterfaceState.Particles!)
            {
                p.Update();
            }
            if (AutoPausing || !InterfaceState.Controls.AllowsUnpause)
            {
                return;
            }
            DateTime now = DateTime.Now;
            int millis = (int)now.Subtract(LastUpdate).TotalMilliseconds;
            decimal fraction = (decimal)Speeds[SpeedIndex].numerator / (decimal)Speeds[SpeedIndex].denominator;
            if (millis > 1000 * fraction)
            {
                InterfaceState.Commands!.AutoWait();
            }

        }
    }
}
