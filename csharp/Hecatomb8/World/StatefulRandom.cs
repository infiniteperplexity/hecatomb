﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public class StatefulRandom
    {
        public int Seed;
        public long Calls;
        private Random? random;
        private Random? stateless;


        public static int GetTimeSeed()
        {
            return
                System.DateTime.Now.Millisecond
                + 1000 * System.DateTime.Now.Second
                + 60 * 1000 * System.DateTime.Now.Minute
                + 60 * 60 * 1000 * System.DateTime.Now.Hour;
        }
        public StatefulRandom(int seed, int calls = 0)
        {
            Seed = seed;
            Calls = calls;
            random = new Random(Seed);
            stateless = new Random();
        }

        public void Reinitialize()
        {
            random = new Random(Seed);
            for (int i = 0; i < Calls; i++)
            {
                random.Next();
            }
        }

        public int Next(int i)
        {
            Calls += 1;
            int next = random!.Next(i);
            return next;
        }

        public int Next(int i, int j)
        {
            Calls += 1;
            int next = random!.Next(i, j);
            return next;
        }

        public int StatelessNext(int i)
        {
            int next = stateless!.Next(i);
            return next;
        }

        public int StatelessNext(int i, int j)
        {
            int next = stateless!.Next(i, j);
            return next;
        }

        public double NextDouble()
        {
            Calls += 1;
            return random!.NextDouble();
        }

        public double StatelessDouble()
        {
            return stateless!.NextDouble();
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
            double u1 = 1.0 - stateless!.NextDouble();
            double u2 = 1.0 - stateless!.NextDouble();
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
            double m = Math.Pow(2, 31);
            long n = (a * seed + c) % (long)m;
            double f = (double)n;
            f /= (double)2147473647.0;
            return f;
        }

        public int Arbitrary(int i, int seed)
        {
            // somehow this occasionally escapes the bounds
            int n = (int)Math.Floor(Arbitrary(seed) * i);
            if (n < 0)
            {
                n = 0;
            }
            else if (n >= i)
            {
                n = i - 1;
            }
            return n;
        }

        public int Arbitrary(int i, int j, int seed)
        {
            int n = (int)Math.Floor(Arbitrary(seed) * j) + i;
            if (n < i)
            {
                n = i;
            }
            else if (n >= j)
            {
                n = j - 1;
            }
            return n;
        }
    }
}
