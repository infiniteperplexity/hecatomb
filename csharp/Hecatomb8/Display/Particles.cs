using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb8
{
    public class ParticleEmitter
    {
        public int X;
        public int Y;
        public int Z;
        public char[] Symbols;
        public string[]? FGs;
        public string[]? BGs;
        public DateTime T0;
        public int LifeSpan;
        public int Rate;
        public DateTime LastEmit;
        public string FG;
        public int V;
        public int A;
        public int Lifespan;
        public int D;

        public ParticleEmitter()
        {
            Symbols = new char[] {
                //'\u26E7', // pentagram
                //'\u2135', // aleph
                //'\u2625', // ankh
                //'\u03BE', // xi
                //'\u26B1', // funeral urn
                //'\u2620' // skull
                '\u037C','\u037D','\u03A6','\u03A8','\u03A9','\u03B1',
                '\u03B3','\u03B4','\u03B6'

            };
            T0 = DateTime.Now;
            Rate = 10;
            InterfaceState.Emitters.Add(this);
            LastEmit = DateTime.Now;
            FG = "black";
            V = 16;
            A = -32;
            LifeSpan = 250;
            D = 0;
        }

        public virtual void Update()
        {
            int millis;
            millis = (int)DateTime.Now.Subtract(LastEmit).TotalMilliseconds;
            if (millis > Rate)
            {
                for (int i = 0; i < millis / Rate; i++)
                {
                    Emit();
                }
            }
            millis = (int)DateTime.Now.Subtract(T0).TotalMilliseconds;
            if (millis > LifeSpan)
            {
                InterfaceState.Emitters.Remove(this);
            }
        }

        public virtual void Place(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            LastEmit = DateTime.Now;
        }
        public virtual void Emit()
        {
            LastEmit = DateTime.Now;
            Particle p = new Particle()
            {
                Symbol = Symbols[GameState.World!.Random.StatelessNext(Symbols.Length)],
                FG = FG,
                X0 = X,
                Y0 = Y,
                Z0 = Z,
                V = V,
                A = A,
                LifeSpan = LifeSpan,
                D = D,
                Angle = (float)(GameState.World!.Random.StatelessDouble() * 2 * Math.PI)
            };
            p.Place(X, Y, Z);
        }
    }

    public class Particle
    {
        public float D;
        public int X0;
        public int Y0;
        public int Z0;
        public float V;
        public float A;
        public float Angle;
        public float Incline;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public char Symbol;
        public string? FG;
        public string? BG;
        public bool Placed;
        public DateTime T0;
        public int LifeSpan;

        public Particle()
        {
            D = 0;
            A = 0;
            V = 0;
            Incline = 0;
            X = -1;
            Y = -1;
            Z = -1;
            T0 = DateTime.Now;
            Placed = false;
            LifeSpan = 1000;
        }

        public void Place(int _x, int _y, int _z)
        {
            if (Placed)
            {
                Remove();
            }
            Placed = true;
            X = _x;
            Y = _y;
            Z = _z;
            InterfaceState.Particles![_x, _y, _z] = InterfaceState.Particles[_x, _y, _z].Concat(new Particle[] { this }).ToList();
        }

        public void Remove()
        {
            if (!Placed)
            {
                return;
            }
            var _x = X;
            var _y = Y;
            var _z = Z;
            InterfaceState.Particles![X, Y, Z] = InterfaceState.Particles[X, Y, Z].Where(p => p != this).ToList();;
            Placed = false;

        }

        public virtual void Update()
        {
            if (this is Highlight)
            {
                return;
            }
            int T = (int)DateTime.Now.Subtract(T0).TotalMilliseconds;
            InterfaceState.DirtifyTile(X, Y, Z);
            if (T > LifeSpan && LifeSpan != int.MaxValue)
            {
                Remove();
                return;
            }
            float t = ((float)T) / 1000f;
            int x = X0 + (int)(Math.Cos(Angle) * (D + V * t + 0.5 * A * t * t));
            int y = Y0 + (int)(Math.Sin(Angle) * (D + V * t + 0.5 * A * t * t));
            int z = Z0 + (int)(Math.Sin(Incline) * (D + V * t + 0.5 * A * t * t));
            Place(x, y, z);
            InterfaceState.DirtifyTile(x, y, z);
        }
    }
    // is there a good way to handle emitting with random symbols?  well...we need emitters...
    public class Highlight : Particle
    {
        public Highlight(string s)
        {
            BG = s;
            LifeSpan = int.MaxValue;
        }
    }

    public class BloodEmitter : ParticleEmitter
    {
        public BloodEmitter()
        {
            Symbols = new char[] { '\u2022' };
            Rate = 10;
            LifeSpan = 50;
            InterfaceState.Emitters.Add(this);
            FG = "red";

        }
    }
}