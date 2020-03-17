/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/10/2018
 * Time: 11:00 AM
 */
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Hecatomb
{
	public class StatefulRandom
	{
		public int Seed;
		public long Calls;
        public int Last;
		private Random random;
        private Random stateless;
		

        public static int GetTimeSeed()
        {
            return 
                System.DateTime.Now.Millisecond
                + 1000 * System.DateTime.Now.Second
                + 60 * 1000 * System.DateTime.Now.Minute
                + 60 * 60 * 1000 * System.DateTime.Now.Hour;
        }
		public StatefulRandom(int seed, int calls=0)
		{
			Seed = seed;
			Calls = calls;
			Initialize();
		}
		
		public void Initialize()
		{
			random = new Random(Seed);
			for (int i=0; i<Calls; i++)
			{
				random.Next();
			}
            stateless = new Random();
		}
		
        public void Poll()
        {
            Debug.WriteLine($"Turn: {Game.World.Turns.Turn}, Last: {Game.World.Random.Last}, Calls: {Game.World.Random.Calls}, Seed: {Game.World.Random.Seed}");
        }

        [JsonIgnore] TurnHandler _cached;
        [JsonIgnore] TurnHandler cachedTurns
        {
            get
            {
                if (_cached != null)
                {
                    return _cached;
                }
                else
                {
                    _cached = Game.World.GetState<TurnHandler>();
                    return _cached;
                }
            }
        }
		public int Next(int i)
		{
            Calls +=1;
            int next = random.Next(i);
            if (cachedTurns.Turn > 0)
            {
                PrintTrace();
            }
            Last = next;
            return next;
		}

        public int Next(int i, int j)
        {
            Calls += 1;
            int next = random.Next(i, j);
            if (cachedTurns.Turn > 0)
            {
                PrintTrace();
            }
            Last = next;
            return next;
        }

        public int StatelessNext(int i)
        {
            int next = stateless.Next(i);
            return next;
        }

        public int StatelessNext(int i, int j)
        {
            int next = stateless.Next(i, j);
            return next;
        }

        public double NextDouble()
		{
            Calls += 1;
            if (cachedTurns.Turn > 0)
            {
                PrintTrace();
            }
            return random.NextDouble();
		}

        public double StatelessDouble()
        {
            return stateless.NextDouble();
        }

        public double NextNormal(float u, float std)
        {
            double u1 = 1.0 - NextDouble();
            double u2 = 1.0 - NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (u + std * randStdNormal);
        }

        public double NextNormal(int u, int std)
        {
            return NextNormal((float)u, (float)std);
        }

        public double StatelessNormal(float u, float std)
        {
            double u1 = 1.0 - stateless.NextDouble();
            double u2 = 1.0 - stateless.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (u + std * randStdNormal);
        }

        public double StatelessNormal(int u, int std)
        {
            return StatelessNormal((float)u, (float)std);
        }

        public double NextNormal()
        {
            return NextNormal(0f, 1f);
        }

        public int Perturb()
        {
            return Next(2) - Next(2);
        }
        public int Perturb(int n)
        {
            return Next(n) - Next(n);
        }

        // used for things that should be arbitrary, repeatable, and not increment the World's random state
        // e.g. perturbing ingredient paths for menu display
        // I believe this is based on the ANSI C PRNG
        public double Arbitrary(int seed)
        {
            long a = 1103515245;
            long c = 12345;
            double m = Math.Pow(2,31);
            long n = (a * seed + c) % (long) m;
            double f = (double)n;
            f /= (double)2147473647.0;
            return f;
        }

        public int Arbitrary(int i, int seed)
        {
            return (int) Math.Floor(Arbitrary(seed) * i);
        }

        public int Arbitrary(int i, int j, int seed)
        {
            return (int)Math.Floor(Arbitrary(seed) * j) + i;
        }

        public void PrintTrace()
        {
            //if (Game.World.Turns.Turn < 69)
            //{
             //   return;
            //}
            //return;
            StackTrace s = new StackTrace();
            if (s.GetFrame(2).GetMethod().Name == "WalkRandom")
            {
                return;
            }
            Debug.WriteLine(
                $"{s.GetFrame(1).GetMethod().Name} called by {s.GetFrame(2).GetMethod().Name}, {s.GetFrame(2).GetMethod().ReflectedType.Name}");
        }
    }
}
