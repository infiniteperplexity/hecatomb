/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/10/2018
 * Time: 11:00 AM
 */
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of GameRandom.
	/// </summary>
	public class GameRandom
	{
		public int Seed;
		public int Calls;
		private Random random;
		
		public GameRandom(int seed, int calls=0)
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
		}
		
		public int Next(int i)
		{
			Calls+=1;
			return random.Next(i);
		}
	}
}
