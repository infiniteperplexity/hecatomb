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

namespace Hecatomb
{
	/// <summary>
	/// Description of Particles.
	/// </summary>
	public class Particles
	{
		public Particles()
		{
		}
	}
	
	public class Particle
	{
		public int x {get; private set;}
		public int y {get; private set;}
		public int z {get; private set;}
		public char Symbol;
		public string FG;
		public string BG;
		public Particle()
		{
			x = -1;
			y = -1;
			z = -1;
		}
		
		public void Place(int _x, int _y, int _z)
		{
			x = _x;
			y = _y;
			z = _z;
			Game.MainPanel.Particles[x, y, z] = this;
		}
		
		public void Remove()
		{
			Game.MainPanel.Particles[x, y, z] = null;
			x = -1;
			y = -1;
			z = -1;
		}
		
		public virtual void Update()
		{
			
		}
	}
	
	public class Highlight : Particle
	{
		public Highlight(string s) : base()
		{
			BG = s;
		}
	}
}
