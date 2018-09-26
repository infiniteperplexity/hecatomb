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
		List<Creature> Minions;
		public Player(string t) : base(t)
		{
			Minions = new List<Creature>();
		}
		public override void OnSelfEvent(PlaceEvent e)
		{
			Debug.Print("The player was placed at {0} {1} {2}", e.x, e.y, e.z);
		}
	}
}
