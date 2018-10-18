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
		char[] Symbols;
		string[] FGs;
		string[] BGs;
		DateTime T0;
		int LifeSpan;
		public int Rate;
		DateTime LastEmit;
		
		public ParticleEmitter()
		{
			T0 = DateTime.Now;
			Rate = 10;
			LifeSpan = 1000;
			Game.MainPanel.Emitters.Add(this);
			Emit();
		}
		
		public virtual void Update()
		{
			int millis;
			millis = (int) DateTime.Now.Subtract(LastEmit).TotalMilliseconds;
			if (millis>Rate)
			{
				Emit();
			}
			millis = (int) DateTime.Now.Subtract(T0).TotalMilliseconds;
			if (millis>LifeSpan)
			{
				Debug.WriteLine("removing emitter");
				Game.MainPanel.Emitters.Remove(this);
			}
		}
		
		public virtual void Place(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		public virtual void Emit()
		{
			LastEmit = DateTime.Now;
			
			Particle p = new Particle()
			{
				Symbol = '@',
				FG = "yellow"
			};
			p.Place(X, Y, Z);
		}
	}
	
	public class Particle
	{
		public int X0;
		public int Y0;
		public int Z0;
		public int V0;
		public int A;
		public int Angle;
		public int Incline;
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
			A = 0;
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
			Game.MainPanel.Particles[_x, _y, _z] = Game.MainPanel.Particles[_x,_y,_z].Concat(new Particle[] {this}).ToList();
		}
		
		public void Remove()
		{
			Game.MainPanel.Particles[X, Y, Z] = Game.MainPanel.Particles[X, Y, Z].Where(p=>p!=this).ToList();
			X = -1;
			Y = -1;
			Z = -1;
			Placed = false;
			
		}
		
		public virtual void Update()
		{
			int T = (int) DateTime.Now.Subtract(T0).TotalMilliseconds;
			Game.MainPanel.DirtifyTile(X, Y, Z);
			if (T>LifeSpan && LifeSpan!=int.MaxValue)
			{
				Remove();
				return;
			}
//			int x = (int) (X0 + Math.Cos(Angle)*(V0*T + 0.5*A*T*T));
//			int y = (int) (Y0 + Math.Sin(Angle)*(V0*T + 0.5*A*T*T));
//			int z = (int) (Z0 + Math.Sin(Incline)*(V0*T + 0.5*A*T*T));
////			Place(x, y, z);
//			Game.MainPanel.DirtifyTile(x, y, z);
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
}
//
//- There's true or false for being in the initial paused state.
//- timePassing sort of doubles as a flag and a reference for disabling recursion.
//- Speeds go from 1/4 to 8/1, using kind of a silly fractional scale.
//- This is multiplied by 1000 milliseconds.
//- ToggleTime is a command.
//- PassTime is I think what happens if you just wait, and it calls a command called Autowait.
//	- Autowait calls the cursor and tells it to hover.
//- ParticleTime is completely independent, but will generally update alternately asynchronously.
//- StartParticles and StopParticles are things...what calls them?  They demand their own updates, although we might blend that in a bit.
//- An alert stops the particles...the only thing that starts them is adding an emitter, so I think we're gonna refactor that.
//- lockTime is a special state; it prevents unpausing.
//- The whole closure space is essentially a "timehandler".
