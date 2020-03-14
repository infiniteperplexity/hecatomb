﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/10/2018
 * Time: 11:00 AM
 */
using System;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{
	public class StatefulRandom
	{
		public int Seed;
		public int Calls;
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
		
		public int Next(int i)
		{
			Calls+=1;
            int next = random.Next(i);
            Debug.WriteLine("R: " + next);
            return next;
		}

        public int Next(int i, int j)
        {
            Calls += 1;
            return random.Next(i, j);
        }


        public double NextDouble()
		{
			Calls+=1;
			return random.NextDouble();
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
	}
}
