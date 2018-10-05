/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/26/2018
 * Time: 10:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Player.
	/// </summary>
	public class Player : Creature
	{
		public List<Creature> Minions;
		public bool Acted;
		public Player(string t) : base(t)
		{
			Acted = false;
			Minions = new List<Creature>();
			
		}
	}
}
