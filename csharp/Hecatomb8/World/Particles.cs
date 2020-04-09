/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/7/2018
 * Time: 9:54 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of Particles.
	/// </summary>
	/// 
	public class ParticleEmitter
    {
        public int X;
        public int Y;
        public int Z;
        public char[] Symbols;
        public string[] FGs;
        public string[] BGs;
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
            LifeSpan = 650;
            Game.World.Emitters.Add(this);
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
            millis = (int) DateTime.Now.Subtract(LastEmit).TotalMilliseconds;
            if (millis>Rate)
            {
                for (int i=0; i<millis/Rate; i++)
                {
                    Emit();
                }
            }
            millis = (int) DateTime.Now.Subtract(T0).TotalMilliseconds;
            if (millis>LifeSpan)
            {
                Game.World.Emitters.Remove(this);
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
//            Debug.WriteLine("emitting");
            LastEmit = DateTime.Now;
            Particle p = new Particle()
            {
                Symbol = Symbols[Game.World.Random.StatelessNext(Symbols.Length)],
                FG = FG,
                X0 = X,
                Y0 = Y,
                Z0 = Z,
//                V = -10,
                V = V,
                A = A,
                LifeSpan = LifeSpan,
//              D = (float) Game.World.Random.Next(1,4),
                D = D,
                Angle = (float) (Game.World.Random.StatelessDouble()*2*Math.PI)
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
        public int X {get; private set;}
        public int Y {get; private set;}
        public int Z {get; private set;}
        public char Symbol;
        public string FG;
        public string BG;
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
            Game.World.Particles[_x, _y, _z] = Game.World.Particles[_x,_y,_z].Concat(new Particle[] {this}).ToList();
        }
        
        public void Remove()
        {
            if (!Placed)
            {
                return;
            }
            Game.World.Particles[X, Y, Z] = Game.World.Particles[X, Y, Z].Where(p=>p!=this).ToList();
            X = -1;
            Y = -1;
            Z = -1;
            Placed = false;
            
        }
        
        public virtual void Update()
        {
            if (this is Highlight)
            {
                return;
            }
            int T = (int) DateTime.Now.Subtract(T0).TotalMilliseconds;
            Game.MainPanel.DirtifyTile(X, Y, Z);
            if (T>LifeSpan && LifeSpan!=int.MaxValue)
            {
                Remove();
                return;
            }
            float t = ((float) T)/1000f;
            int x = X0 + (int) (Math.Cos(Angle)*(D+V*t + 0.5*A*t*t));
            int y = Y0 + (int) (Math.Sin(Angle)*(D+V*t + 0.5*A*t*t));
            int z = Z0 + (int) (Math.Sin(Incline)*(D+V*t + 0.5*A*t*t));
            Place(x, y, z);
            Game.MainPanel.DirtifyTile(x, y, z);
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
            Game.World.Emitters.Add(this);
            FG = "red";

        }
    }

}

